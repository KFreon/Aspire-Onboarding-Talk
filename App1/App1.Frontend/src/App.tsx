import { useState } from "react";
import ProductList from "./components/ProductList";
import ProductForm from "./components/ProductForm";
//@ts-ignore can't be bothered to fix
import "./App.css";

export default function App() {
  const [refreshKey, setRefreshKey] = useState(0);

  return (
    <div className="app">
      <h1>Products (App1 - SQL Server)</h1>
      <ProductForm onCreated={() => setRefreshKey((k) => k + 1)} />
      <ProductList key={refreshKey} onDeleted={() => setRefreshKey((k) => k + 1)} />
    </div>
  );
}
