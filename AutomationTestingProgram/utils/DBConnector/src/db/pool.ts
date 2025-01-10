import { Pool } from "pg";

const pool = new Pool({
    user: "postgres",
    host: "on34c03370053",
    database: "automationtestingprogram",
    password: "password",
    port: 5432,
});

pool.connect()
    .then(() => console.log('Connected to PostgreSQL'))
    .catch((err: any) => console.error('Error connecting to PostgreSQL:', err));

export default pool;