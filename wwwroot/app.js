
const api = "/api";

async function fetchMedicines(q="") {
  const url = q ? `${api}/medicines?q=${encodeURIComponent(q)}` : `${api}/medicines`;
  const res = await fetch(url);
  return await res.json();
}

function rowClass(m) {
  // robust expiry parsing and numeric handling
  const qty = Number(m.quantity ?? 0);
  const expiryTimestamp = Date.parse(m.expiryDate);
  if (!isNaN(expiryTimestamp)) {
    const expiry = new Date(expiryTimestamp);
    const days = (expiry - new Date())/(1000*60*60*24);
    if (days < 30) return "red";
  }
  if (!isNaN(qty) && qty < 10) return "yellow";
  return "";
}

function render(list) {
  const tbody = document.querySelector("#grid tbody");
  tbody.innerHTML = "";
  list.forEach(m => {
    const tr = document.createElement("tr");
    const cls = rowClass(m);
    const priceNum = Number(m.price ?? 0);
    const priceText = isNaN(priceNum) ? '0.00' : priceNum.toFixed(2);
    const expiryText = m.expiryDate ? new Date(m.expiryDate).toLocaleDateString() : '';
    tr.innerHTML = `<td>${m.fullName ?? ''}</td>
                    <td>${expiryText}</td>
                    <td>${Number(m.quantity ?? 0)}</td>
                    <td>${priceText}</td>
                    <td>${m.brand ?? ""}</td>
                    <td><button class="sell" data-id="${m.id}">Sell</button></td>`;
    tbody.appendChild(tr);
    // Apply class after row HTML is set to ensure styles apply in all browsers
    if (cls) {
      tr.classList.add(cls);
      // also add class to each cell for extra robustness
      tr.querySelectorAll('td').forEach(td => td.classList.add(cls));
    }
  });
  document.querySelectorAll(".sell").forEach(btn=>{
    btn.onclick = async ()=> {
      const id = btn.dataset.id;
      const qty = parseInt(prompt("Quantity to sell", "1"));
      if (!qty) return;
      const res = await fetch(`${api}/sales`, {method:"POST", headers:{"content-type":"application/json"}, body: JSON.stringify({ medicineId: id, quantity: qty })});
      if (res.ok) {
        alert("Sale recorded");
        load();
      } else {
        const err = await res.json();
        alert("Error: " + (err?.error ?? JSON.stringify(err)));
      }
    }
  });
}

async function load(q="") {
  const list = await fetchMedicines(q);
  render(list);
}

function clearFormErrors() {
  document.getElementById('err_name').textContent = '';
  document.getElementById('err_expiry').textContent = '';
  document.getElementById('err_qty').textContent = '';
  document.getElementById('err_price').textContent = '';
  document.getElementById('err_notes').textContent = '';
  document.getElementById('err_brand').textContent = '';
}

function validateForm() {
  clearFormErrors();
  let ok = true;
  const name = document.getElementById('m_name').value.trim();
  const expiry = document.getElementById('m_expiry').value;
  const qty = parseInt(document.getElementById('m_qty').value || '0');
  const price = parseFloat(document.getElementById('m_price').value || '0');
  const notes = document.getElementById('m_notes').value.trim();
  const brand = document.getElementById('m_brand').value.trim();
  if (!name) { document.getElementById('err_name').textContent = 'Name is required'; ok = false; }
  if (!expiry || isNaN(Date.parse(expiry))) { document.getElementById('err_expiry').textContent = 'Valid expiry date required'; ok = false; }
  else {
    // expiry should be in the future (or at least today)
    const expDate = new Date(expiry);
    const today = new Date();
    today.setHours(0,0,0,0);
    if (expDate < today) { document.getElementById('err_expiry').textContent = 'Expiry must be today or later'; ok = false; }
  }
  if (isNaN(qty) || qty < 0) { document.getElementById('err_qty').textContent = 'Quantity must be 0 or more'; ok = false; }
  if (isNaN(price) || price < 0) { document.getElementById('err_price').textContent = 'Price must be 0.00 or more'; ok = false; }
  if (!notes) { document.getElementById('err_notes').textContent = 'Notes are required'; ok = false; }
  if (!brand) { document.getElementById('err_brand').textContent = 'Brand is required'; ok = false; }
  return ok;
}

document.getElementById("refresh").addEventListener("click", ()=> load(document.getElementById("search").value));
document.getElementById("search").addEventListener("input", (e)=> load(e.target.value));
document.getElementById("addBtn").addEventListener("click", ()=> document.getElementById("formModal").classList.remove("hidden"));
document.getElementById("cancel").addEventListener("click", ()=> document.getElementById("formModal").classList.add("hidden"));
document.getElementById("save").addEventListener("click", async ()=>{
  if (!validateForm()) return;
  const payload = {
    fullName: document.getElementById("m_name").value.trim(),
    notes: document.getElementById("m_notes").value.trim(),
    expiryDate: document.getElementById("m_expiry").value,
    quantity: parseInt(document.getElementById("m_qty").value || "0"),
    price: Math.round(parseFloat(document.getElementById("m_price").value || "0") * 100) / 100,
    brand: document.getElementById("m_brand").value.trim()
  };
  const res = await fetch(`${api}/medicines`, {method:"POST", headers:{"content-type":"application/json"}, body: JSON.stringify(payload)});
  if (res.ok) {
    alert("Added");
    document.getElementById("formModal").classList.add("hidden");
    // clear form
    document.getElementById('m_name').value = '';
    document.getElementById('m_notes').value = '';
    document.getElementById('m_expiry').value = '';
    document.getElementById('m_qty').value = '';
    document.getElementById('m_price').value = '';
    document.getElementById('m_brand').value = '';
    load();
  } else {
    const err = await res.json().catch(()=>null);
    alert("Failed to add: " + (err?.error ?? JSON.stringify(err) ?? res.statusText));
  }
});

load();
