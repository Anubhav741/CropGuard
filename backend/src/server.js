import 'dotenv/config';
import { PrismaClient } from '@prisma/client';
import { createApp } from './app.js';
const db=new PrismaClient();
const app=createApp({db,jwtSecret:process.env.JWT_SECRET,frontendUrl:process.env.FRONTEND_URL});
const server=app.listen(Number(process.env.PORT||5000),()=>console.log(`CropGuard API listening on ${process.env.PORT||5000}`));
for(const sig of ['SIGINT','SIGTERM'])process.on(sig,()=>server.close(async()=>{await db.$disconnect();process.exit(0);}));
