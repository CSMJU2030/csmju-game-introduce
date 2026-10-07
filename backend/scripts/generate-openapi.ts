import 'reflect-metadata';
import { writeFileSync } from 'node:fs';
import { NestFactory } from '@nestjs/core';
import { SwaggerModule, DocumentBuilder } from '@nestjs/swagger';
import { AppModule } from '../src/app.module';
import { ROUTES_OUTSIDE_API_PREFIX } from '../src/app-setup';

// Metadata generation only: no listen/init and no connection to a live database.
process.env.NODE_ENV = 'development';
process.env.DATABASE_URL ??= 'postgresql://build:build@localhost:5432/build_only';

async function generate() {
  const app = await NestFactory.create(AppModule, { logger: false });
  app.setGlobalPrefix('api', { exclude: ROUTES_OUTSIDE_API_PREFIX });
  const config = new DocumentBuilder().setTitle('CSMJU Game Introduce API').setVersion('1.0.0').addBearerAuth().build();
  const document = SwaggerModule.createDocument(app, config);
  writeFileSync('openapi.json', JSON.stringify(document, null, 2) + '\n');
  await app.close();
}
void generate();
