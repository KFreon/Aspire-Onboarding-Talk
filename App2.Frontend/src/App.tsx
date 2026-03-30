import { useState } from "react";
import OrderList from "./components/OrderList";
import OrderForm from "./components/OrderForm";
import "./App.css";

export default function App() {
  const [refreshKey, setRefreshKey] = useState(0);

  return (
    <div className="app">
      <h1>Orders (App2 - PostgreSQL)</h1>
      <OrderForm onCreated={() => setRefreshKey((k) => k + 1)} />
      <OrderList key={refreshKey} onDeleted={() => setRefreshKey((k) => k + 1)} />
    </div>
  );
}
