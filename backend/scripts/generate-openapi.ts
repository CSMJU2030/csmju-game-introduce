import 'reflect-metadata';
import { writeFileSync } from 'node:fs';
import { NestFactory } from '@nestjs/core';
import { SwaggerModule, DocumentBuilder } from '@nestjs/swagger';
import { config as loadEnv } from 'dotenv';
import { ROUTES_OUTSIDE_API_PREFIX } from '../src/app-setup';

// Metadata generation only: no listen/init and no connection to a live database.
loadEnv({path:'.env.example',quiet:true});
process.env.NODE_ENV = 'development';

async function generate() {
  const { AppModule } = await import('../src/app.module');
  const app = await NestFactory.create(AppModule, { logger: false });
  app.setGlobalPrefix('api', { exclude: ROUTES_OUTSIDE_API_PREFIX });
  const config = new DocumentBuilder().setTitle('CSMJU Game Introduce API').setVersion('1.0.0').addBearerAuth().build();
  const document = SwaggerModule.createDocument(app, config);
  writeFileSync('openapi.json', JSON.stringify(document, null, 2) + '\n');
  await app.close();
}
void generate();
