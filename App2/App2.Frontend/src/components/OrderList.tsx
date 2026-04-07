import { useEffect, useState } from "react";

interface Order {
  id: number;
  customerName: string;
  customerEmail: string;
  productId: number;
  quantity: number;
  totalPrice: number;
  orderDate: string;
  status: string;
}

export default function OrderList({ onDeleted }: { onDeleted: () => void }) {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("/api/orders")
      .then((res) => res.json())
      .then(setOrders)
      .finally(() => setLoading(false));
  }, []);

  const handleDelete = async (id: number) => {
    await fetch(`/api/orders/${id}`, { method: "DELETE" });
    onDeleted();
  };

  if (loading) return <div className="loading">Loading orders...</div>;

  return (
    <div>
      <h2>All Orders ({orders.length})</h2>
      <table>
        <thead>
          <tr>
            <th>Customer</th>
            <th>Email</th>
            <th>Product ID</th>
            <th>Qty</th>
            <th>Total</th>
            <th>Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {orders.map((o) => (
            <tr key={o.id}>
              <td>{o.customerName}</td>
              <td>{o.customerEmail}</td>
              <td>{o.productId}</td>
              <td>{o.quantity}</td>
              <td>${o.totalPrice.toFixed(2)}</td>
              <td>{o.status}</td>
              <td>
                <button className="delete-btn" onClick={() => handleDelete(o.id)}>
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
