CREATE TABLE "Orders" (
    "Id" SERIAL PRIMARY KEY,
    "CustomerName" VARCHAR(200) NOT NULL,
    "CustomerEmail" VARCHAR(200) NOT NULL,
    "ProductId" INT NOT NULL,
    "Quantity" INT NOT NULL,
    "TotalPrice" DECIMAL(18,2) NOT NULL,
    "OrderDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending'
);
