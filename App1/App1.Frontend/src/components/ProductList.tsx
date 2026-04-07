import { useEffect, useState } from "react";

interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  createdAt: string;
}

export default function ProductList({ onDeleted }: { onDeleted: () => void }) {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("/api/products")
      .then((res) => res.json())
      .then(setProducts)
      .finally(() => setLoading(false));
  }, []);

  const handleDelete = async (id: number) => {
    await fetch(`/api/products/${id}`, { method: "DELETE" });
    onDeleted();
  };

  if (loading) return <div className="loading">Loading products...</div>;

  return (
    <div>
      <h2>All Products ({products.length})</h2>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th>Price</th>
            <th>Stock</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.id}>
              <td>{p.name}</td>
              <td>{p.description}</td>
              <td>${p.price.toFixed(2)}</td>
              <td>{p.stockQuantity}</td>
              <td>
                <button className="delete-btn" onClick={() => handleDelete(p.id)}>
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
