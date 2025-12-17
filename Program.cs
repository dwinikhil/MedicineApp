using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Ensure JSON uses camelCase to match the frontend JavaScript expectations
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opts.SerializerOptions.WriteIndented = false;
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles(); // serve index.html from wwwroot
app.UseStaticFiles();

string dataFile = Path.Combine(app.Environment.ContentRootPath, "Data", "medicines.json");
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Data"));

// Thread-safe in-memory cache with persistence to JSON file.
var store = new MedicineStore(dataFile);

// API endpoints
app.MapGet("/api/medicines", (HttpRequest req) =>
{
    var q = req.Query["q"].ToString();
    var items = store.GetAll();
    if (!string.IsNullOrWhiteSpace(q))
    {
        // Guard against possible null FullName values in stored items.
        items = items.Where(m => !string.IsNullOrWhiteSpace(m.FullName) && m.FullName.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    }
    return Results.Ok(items);
});

app.MapGet("/api/medicines/{id}", (Guid id) =>
{
    var m = store.Get(id);
    return m is not null ? Results.Ok(m) : Results.NotFound();
});

app.MapPost("/api/medicines", async (Medicine incoming) =>
{
    // Basic validation: FullName is required.
    if (string.IsNullOrWhiteSpace(incoming.FullName))
    {
        return Results.BadRequest(new { error = "FullName is required" });
    }
    if (incoming.Quantity < 0) return Results.BadRequest(new { error = "Quantity must be non-negative" });
    if (incoming.Price < 0) return Results.BadRequest(new { error = "Price must be non-negative" });
    if (incoming.ExpiryDate == default) return Results.BadRequest(new { error = "ExpiryDate is required" });
    if (string.IsNullOrWhiteSpace(incoming.Notes)) return Results.BadRequest(new { error = "Notes are required" });
    if (string.IsNullOrWhiteSpace(incoming.Brand)) return Results.BadRequest(new { error = "Brand is required" });
    // Normalize price to two decimal places
    incoming.Price = Math.Round(incoming.Price, 2);
    incoming.Id = Guid.NewGuid();
    incoming.CreatedAt = DateTime.UtcNow;
    store.Add(incoming);
    return Results.Created($"/api/medicines/{incoming.Id}", incoming);
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/sales", async (SaleRequest req) =>
{
    var m = store.Get(req.MedicineId);
    if (m is null) return Results.NotFound(new { error = "medicine not found" });
    if (req.Quantity <= 0) return Results.BadRequest(new { error = "invalid quantity" });
    if (m.Quantity < req.Quantity) return Results.BadRequest(new { error = "not enough stock" });
    m.Quantity -= req.Quantity;
    store.Update(m);
    // Append a simple sale record (not exposed via API for simplicity)
    store.AddSale(new SaleRecord { Id = Guid.NewGuid(), MedicineId = m.Id, Quantity = req.Quantity, Price = m.Price, SoldAt = DateTime.UtcNow });
    return Results.Ok(m);
});

app.Run();

// --- Models and store
public class Medicine
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string? Notes { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Brand { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SaleRequest
{
    public Guid MedicineId { get; set; }
    public int Quantity { get; set; }
}

public class SaleRecord
{
    public Guid Id { get; set; }
    public Guid MedicineId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime SoldAt { get; set; }
}

public class MedicineStore
{
    private readonly string _filePath;
    private readonly ConcurrentDictionary<Guid, Medicine> _items = new();
    private readonly object _fileLock = new();

    public MedicineStore(string filePath)
    {
        _filePath = filePath;
        Load();
    }

    private void Load()
    {
        lock (_fileLock)
        {
            List<Medicine> list = new();

            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        list = JsonSerializer.Deserialize<List<Medicine>>(json) ?? new List<Medicine>();
                    }
                    catch (JsonException)
                    {
                        // corrupted file: start with empty list and overwrite below
                        list = new List<Medicine>();
                    }
                }
            }

            // Remove null entries and entries without a valid FullName
            list = list.Where(m => m != null && !string.IsNullOrWhiteSpace(m.FullName)).ToList();

            // If no valid medicines remain, seed a single test medicine so attempts to operate succeed.
            if (list.Count == 0)
            {
                list = new List<Medicine>
                {
                    new Medicine { Id = Guid.NewGuid(), FullName = "Test Medicine 100mg", Notes = "Seeded test item", ExpiryDate = DateTime.UtcNow.AddYears(1), Quantity = 100, Price = 9.99m, Brand = "TestBrand", CreatedAt = DateTime.UtcNow }
                };
            }

            // Persist the cleaned/seeded list back to disk to avoid repeated problems on next startup.
            File.WriteAllText(_filePath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));

            foreach (var m in list) _items[m.Id] = m;
        }
    }

    private void Persist()
    {
        lock (_fileLock)
        {
            var list = _items.Values.OrderBy(x => x.FullName ?? string.Empty).ToList();
            File.WriteAllText(_filePath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public List<Medicine> GetAll() => _items.Values.OrderBy(x => x.FullName ?? string.Empty).ToList();
    public Medicine? Get(Guid id) => _items.TryGetValue(id, out var v) ? v : null;
    public void Add(Medicine m) { _items[m.Id] = m; Persist(); }
    public void Update(Medicine m) { _items[m.Id] = m; Persist(); }

    // Simple sale list persistence
    public void AddSale(SaleRecord r)
    {
        var salesFile = Path.Combine(Path.GetDirectoryName(_filePath)!, "sales.json");
            lock (_fileLock)
            {
                List<SaleRecord> sales = new();
                if (File.Exists(salesFile))
                {
                    var sjson = File.ReadAllText(salesFile);
                    if (string.IsNullOrWhiteSpace(sjson))
                    {
                        sales = new List<SaleRecord>();
                    }
                    else
                    {
                        try
                        {
                            sales = JsonSerializer.Deserialize<List<SaleRecord>>(sjson) ?? new List<SaleRecord>();
                        }
                        catch (JsonException)
                        {
                            // corrupted sales file: reset to empty
                            sales = new List<SaleRecord>();
                        }
                    }
                }
                sales.Add(r);
                File.WriteAllText(salesFile, JsonSerializer.Serialize(sales, new JsonSerializerOptions { WriteIndented = true }));
            }
    }
}
