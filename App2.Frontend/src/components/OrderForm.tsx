import { useState } from "react";

export default function OrderForm({ onCreated }: { onCreated: () => void }) {
  const [customerName, setCustomerName] = useState("");
  const [customerEmail, setCustomerEmail] = useState("");
  const [productId, setProductId] = useState("");
  const [quantity, setQuantity] = useState("");
  const [unitPrice, setUnitPrice] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await fetch("/api/orders", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        customerName,
        customerEmail,
        productId: parseInt(productId),
        quantity: parseInt(quantity),
        unitPrice: parseFloat(unitPrice),
      }),
    });
    setCustomerName("");
    setCustomerEmail("");
    setProductId("");
    setQuantity("");
    setUnitPrice("");
    onCreated();
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Create Order</h2>
      <div>
        <label>Customer Name</label>
        <input value={customerName} onChange={(e) => setCustomerName(e.target.value)} required />
      </div>
      <div>
        <label>Customer Email</label>
        <input type="email" value={customerEmail} onChange={(e) => setCustomerEmail(e.target.value)} required />
      </div>
      <div>
        <label>Product ID</label>
        <input type="number" value={productId} onChange={(e) => setProductId(e.target.value)} required />
      </div>
      <div>
        <label>Quantity</label>
        <input type="number" value={quantity} onChange={(e) => setQuantity(e.target.value)} required />
      </div>
      <div>
        <label>Unit Price</label>
        <input type="number" step="0.01" value={unitPrice} onChange={(e) => setUnitPrice(e.target.value)} required />
      </div>
      <button type="submit">Create Order</button>
    </form>
  );
}
