import {PrismaClient} from '@prisma/client';
const db=new PrismaClient();
try { for(const level of [{id:1,title:'Farm Rescue',cropCount:5,pointsPerCrop:100},{id:2,title:'Moonlight Rescue',cropCount:6,pointsPerCrop:150},{id:3,title:'Village Farm',cropCount:7,pointsPerCrop:200}]) await db.level.upsert({where:{id:level.id},update:level,create:level}); } finally { await db.$disconnect(); }
