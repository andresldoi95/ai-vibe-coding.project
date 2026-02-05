import { Pool, PoolConfig } from 'pg';
import { logger } from './logger';

const poolConfig: PoolConfig = {
  host: process.env.DB_HOST || 'localhost',
  port: parseInt(process.env.DB_PORT || '5432'),
  database: process.env.DB_NAME || 'billing_inventory',
  user: process.env.DB_USER || 'postgres',
  password: process.env.DB_PASSWORD || 'postgres',
  ssl: process.env.DB_SSL === 'true' ? { rejectUnauthorized: false } : false,
  max: 20, // Maximum number of clients in the pool
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 2000,
};

export const pool = new Pool(poolConfig);

// Test database connection
export async function connectDatabase(): Promise<void> {
  try {
    const client = await pool.connect();
    const result = await client.query('SELECT NOW()');
    logger.info(`Database connected at ${result.rows[0].now}`);
    client.release();
  } catch (error) {
    logger.error('Database connection failed:', error);
    throw error;
  }
}

// Handle pool errors
pool.on('error', (err: Error) => {
  logger.error('Unexpected database pool error:', err);
});

// Graceful shutdown
process.on('SIGTERM', async () => {
  await pool.end();
  logger.info('Database pool closed');
});

export default pool;
