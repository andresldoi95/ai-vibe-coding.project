import app from './app';
import { logger } from './config/logger';
import { connectDatabase } from './config/database';

const PORT = process.env.PORT || 3000;

async function startServer() {
  try {
    // Connect to database (optional in development)
    try {
      await connectDatabase();
      logger.info('✓ Database connected successfully');
    } catch (dbError) {
      if (process.env.NODE_ENV === 'production') {
        throw dbError;
      }
      logger.warn('⚠ Database connection failed (continuing without DB in development)');
      logger.warn(`Database error: ${dbError instanceof Error ? dbError.message : 'Unknown error'}`);
    }

    // Start server
    const server = app.listen(PORT, () => {
      logger.info(`✓ Server running on port ${PORT}`);
      logger.info(`✓ Environment: ${process.env.NODE_ENV || 'development'}`);
      logger.info(`✓ SRI Environment: ${process.env.SRI_ENVIRONMENT === '2' ? 'PRODUCCIÓN' : 'PRUEBAS'}`);
      logger.info(`✓ API: http://localhost:${PORT}${process.env.API_PREFIX || '/api/v1'}`);
    });

    // Graceful shutdown
    process.on('SIGTERM', () => {
      logger.info('SIGTERM signal received: closing HTTP server');
      server.close(() => {
        logger.info('HTTP server closed');
        process.exit(0);
      });
    });

    process.on('SIGINT', () => {
      logger.info('SIGINT signal received: closing HTTP server');
      server.close(() => {
        logger.info('HTTP server closed');
        process.exit(0);
      });
    });

  } catch (error) {
    logger.error('Failed to start server:', error);
    process.exit(1);
  }
}

startServer();
