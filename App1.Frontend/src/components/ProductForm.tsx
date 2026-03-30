import { useState } from "react";

export default function ProductForm({ onCreated }: { onCreated: () => void }) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [stockQuantity, setStockQuantity] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await fetch("/api/products", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name,
        description,
        price: parseFloat(price),
        stockQuantity: parseInt(stockQuantity),
      }),
    });
    setName("");
    setDescription("");
    setPrice("");
    setStockQuantity("");
    onCreated();
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Add Product</h2>
      <div>
        <label>Name</label>
        <input value={name} onChange={(e) => setName(e.target.value)} required />
      </div>
      <div>
        <label>Description</label>
        <input value={description} onChange={(e) => setDescription(e.target.value)} />
      </div>
      <div>
        <label>Price</label>
        <input type="number" step="0.01" value={price} onChange={(e) => setPrice(e.target.value)} required />
      </div>
      <div>
        <label>Stock Quantity</label>
        <input type="number" value={stockQuantity} onChange={(e) => setStockQuantity(e.target.value)} required />
      </div>
      <button type="submit">Add Product</button>
    </form>
  );
}
