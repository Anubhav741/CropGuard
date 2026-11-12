import test from 'node:test';
import assert from 'node:assert/strict';
import request from 'supertest';
import {PrismaClient} from '@prisma/client';
import {createApp} from '../src/app.js';
const db=new PrismaClient(); const app=createApp({db,jwtSecret:'a-test-secret-that-is-at-least-32-characters'});
test('authentication, session validation, persistence, and leaderboard',async()=>{
 const email=`test-${Date.now()}@example.com`;let user;
 try {
 let r=await request(app).post('/api/auth/register').send({name:'Test Farmer',email,password:'password123'});assert.equal(r.status,201);assert.equal(r.body.user.passwordHash,undefined);user=r.body.user;const token=r.body.token;
 r=await request(app).post('/api/auth/login').send({email,password:'password123'});assert.equal(r.status,200);
 r=await request(app).get('/api/auth/me').set('Authorization','Bearer invalid');assert.equal(r.status,401);
 r=await request(app).get('/api/game/stats');assert.equal(r.status,401);
 r=await request(app).post('/api/game/start').set('Authorization',`Bearer ${token}`).send({levelId:2});assert.equal(r.status,403);
 r=await request(app).post('/api/game/start').set('Authorization',`Bearer ${token}`).send({levelId:1});assert.equal(r.status,201);const sessionId=r.body.sessionId;
 const result={sessionId,levelId:1,score:9999,cropsTreated:5,livesRemaining:3,duration:0,completed:true};
 r=await request(app).post('/api/game/complete').set('Authorization',`Bearer ${token}`).send(result);assert.equal(r.status,400);
 result.score=500;r=await request(app).post('/api/game/complete').set('Authorization',`Bearer ${token}`).send(result);assert.equal(r.status,200);
 r=await request(app).post('/api/game/complete').set('Authorization',`Bearer ${token}`).send(result);assert.equal(r.status,409);
 r=await request(app).get('/api/game/stats').set('Authorization',`Bearer ${token}`);assert.equal(r.body.highestScore,500);assert.equal(r.body.completedLevels,1);assert.equal(r.body.cropsTreated,5);
 r=await request(app).get('/api/game/history').set('Authorization',`Bearer ${token}`);assert.equal(r.body.items[0].result.score,500);
 r=await request(app).get('/api/game/leaderboard');assert.equal(r.status,200);assert.ok(r.body.items.some(x=>x.id===user.id));
 r=await request(app).get('/api/game/levels').set('Authorization',`Bearer ${token}`);assert.equal(r.body.levels[1].unlocked,true);
 } finally {if(user)await db.user.delete({where:{id:user.id}});}
});
test.after(async()=>db.$disconnect());
