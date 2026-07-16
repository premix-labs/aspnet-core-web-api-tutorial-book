# Teaching Principles for 10/10 Tutorial Books

## Table of Contents

- [Goal](#goal)
- [Definition of Done](#definition-of-done)
- [Chapter Structure](#chapter-structure)
- [Code Teaching Method](#code-teaching-method)
- [10-Point Rubric](#10-point-rubric)
- [Review Checklist](#review-checklist)
- [Common Failure Patterns](#common-failure-patterns)
- [Verification Gates](#verification-gates)

## Goal

หนังสือสอนเขียนเว็บที่ดีต้องทำให้ผู้เรียนสร้างของที่รันได้จริง แต่ไม่ปล่อยให้พิมพ์ตามแบบไม่เข้าใจ ทุกครั้งก่อนใช้ concept ใหม่ เช่น Controller, Service, Repository, DTO, Validation, Authentication, Docker หรือ Deployment ต้องตอบให้ได้ว่า:

- มันแก้ปัญหาอะไร
- ถ้าไม่มีมันจะเกิดปัญหาอะไร
- ในงานจริงใช้มันเพื่ออะไร
- ในบทนี้ใช้เฉพาะส่วนไหนก่อน
- implementation ตอนนี้ยังมีข้อจำกัดอะไร

## Definition of Done

บทต้องสอนได้ครบจากเนื้อหาในหนังสือเอง ผู้เรียนไม่ควรต้องเปิดไฟล์ใน `examples/` เพื่อรู้ว่าต้องเขียนอะไร เขียนที่ไฟล์ไหน หรือตรวจผลอย่างไร ไฟล์ example ใช้เป็น source of truth สำหรับผู้เขียนและ reviewer เพื่อตรวจว่าเนื้อหาในหนังสือถูกต้อง build/test ได้จริง และใช้เป็น optional reference หลังเรียนจบเท่านั้น

บทหนึ่งจะถือว่าพร้อมสอนเมื่อผ่านทุกข้อ:

- มีเป้าหมายบทที่ชัดเจน
- บอก prerequisites ก่อนเริ่ม
- บอกไฟล์ที่จะเปลี่ยนหลังจบบท
- อธิบายคำใหม่, method, attribute, annotation, package หรือ configuration ก่อนใช้
- ถ้าสร้าง folder/file ต้องมีคำสั่งที่ copy ได้
- ถ้าคำสั่งต่างกันตาม OS ต้องมี Windows PowerShell และ macOS/Linux Bash
- โค้ดถูกแบ่งเป็นขั้น ไม่แปะ class หรือ method ยาวรวดเดียว
- code block ยาวไม่เกินประมาณ 30 บรรทัด เว้นแต่มีเหตุผลชัดเจน
- มีคำสั่ง run/test/build สำหรับตรวจผล
- มี expected result เช่น status code, response body, log หรือไฟล์ที่ควรเห็น
- มี common errors หรือคำแนะนำเมื่อผลไม่ตรง
- มี checkpoint ก่อนอ่านบทต่อไป
- เนื้อหาตรงกับ progressive project ณ จุดนั้นของหนังสือ
- example/final/validation project build หรือ test ผ่านตาม scope ของบท

## Chapter Structure

บทที่มีการลงมือเขียนโค้ดควรเรียงแบบนี้:

1. บทนี้จะทำอะไร
2. ก่อนเริ่มบทนี้
3. สิ่งที่จะใช้ในบทนี้
4. หลังจบบทนี้ ไฟล์ที่เปลี่ยน
5. flow หรือ diagram ถ้าช่วยให้เข้าใจ
6. ขั้นที่ 1: สร้าง folder/file
7. ขั้นที่ 2: เพิ่ม code ส่วนเล็ก
8. ขั้นที่ 3: ต่อเข้ากับ service/config/DI
9. ขั้นที่ 4: ทดสอบ
10. ผลลัพธ์ที่ควรเห็น
11. ปัญหาที่พบบ่อย
12. Checkpoint

บทที่เป็น concept ล้วนไม่จำเป็นต้องมีทุกหัวข้อ แต่ยังควรมีเป้าหมาย ตัวอย่าง และ checkpoint

## Code Teaching Method

สอนโค้ดเป็นชั้น:

1. สร้าง folder
2. สร้าง file
3. เพิ่ม using/import
4. เพิ่ม class/record/interface
5. เพิ่ม property หรือ field
6. เพิ่ม constructor หรือ dependency
7. เพิ่ม method ทีละ method
8. ต่อเข้ากับ dependency injection หรือ configuration
9. ต่อเข้ากับ controller/route/endpoint
10. ทดสอบผลลัพธ์

ห้ามให้ผู้เรียนเพิ่มโค้ดใหญ่ทั้งไฟล์โดยไม่มีจุดหยุดอธิบาย ถ้าโค้ดยาว ให้แตกเป็นหลาย block และอธิบายระหว่างทาง

## 10-Point Rubric

| หมวด                                          | คะแนน |
| --------------------------------------------- | ----: |
| เป้าหมายและบริบทชัด                           |     1 |
| อธิบาย concept/method/attribute ก่อนใช้       |     2 |
| ขั้นตอนลงมือทำละเอียดและไม่กระโดด             |     2 |
| โค้ดแตกเป็นส่วนอ่านง่าย                       |     1 |
| คำสั่งตรวจและ expected result ชัด             |   1.5 |
| common errors/checkpoint ช่วยตรวจตัวเองได้    |     1 |
| ตรงกับ example project และ build/test ได้จริง |   1.5 |

การตีความคะแนน:

- 10/10: ผู้เรียนใหม่ทำตามได้ เข้าใจเหตุผล และตรวจงานเองได้
- 8/10: เนื้อหาถูก แต่ยังมีบางช่วงที่ต้องเดาเอง
- 6/10: มีโค้ดและคำอธิบาย แต่ยังเหมือนให้ copy ตาม
- ต่ำกว่า 6/10: เสี่ยงทำให้ผู้เรียนหลงทางหรือเข้าใจผิด

## Review Checklist

ใช้รายการนี้ขณะตรวจทีละบท:

- บทนี้มีเป้าหมายชัดหรือไม่
- ผู้เรียนรู้ไหมว่าต้องเปิดไฟล์ไหน
- ถ้าต้องสร้าง folder/file มีคำสั่งครบหรือไม่
- มีคำสั่งทั้ง Windows และ macOS/Linux เมื่อจำเป็นหรือไม่
- มีคำใหม่ที่ยังไม่ได้อธิบายก่อนใช้หรือไม่
- มีโค้ดยาวเกินไปหรือไม่
- มีจุดที่บอกให้ลบ/แก้โค้ด แต่ไม่บอกโค้ดที่เหลือควรเป็นอย่างไรหรือไม่
- route, port, filename และ project name ตรงกับของจริงหรือไม่
- expected response/status code ถูกต้องหรือไม่
- checkpoint ตรวจได้จริงหรือเป็นแค่ข้อความกว้าง ๆ
- มีคำเตือนเรื่อง demo vs production เมื่อใช้วิธีที่ยังไม่ปลอดภัยหรือไม่
- progressive project ทำตามบทนี้แล้ว compile/test ผ่านหรือไม่
- final project สอดคล้องกับบทนี้หรือมีเหตุผลเมื่อแตกต่างหรือไม่

## Common Failure Patterns

ถ้าเจอสิ่งเหล่านี้ ให้ถือว่าเป็นจุดที่ต้องขัดเกลา:

- แปะโค้ดยาวทั้งไฟล์โดยไม่อธิบาย
- ใช้คำว่า "เพิ่มโค้ดนี้" แต่ไม่บอกตำแหน่ง
- ใช้ class หรือ field ที่ยังไม่เคยสร้าง
- ใช้ package/library แต่ไม่บอกคำสั่งติดตั้ง
- สอน command ที่ใช้ได้เฉพาะ OS เดียวโดยไม่บอก
- response ตัวอย่างไม่ตรงกับโค้ดจริง
- port ในหนังสือไม่ตรงกับ launch settings หรือ project จริง
- checkpoint ตรวจไม่ได้จริง
- warning/error ที่ผู้เรียนเจอบ่อยไม่มีคำอธิบาย
- production warning ไม่ชัด เช่น secret, CORS เปิดกว้าง, logging sensitive data

## Verification Gates

ก่อนบอกว่างานขัดเกลาเสร็จ ให้ตรวจตาม scope:

- book/docs changes: `npm run build`
- ASP.NET example changes: relevant `dotnet test`; run `dotnet publish` when deployment/runtime behavior changes
- Spring Boot example changes: relevant Gradle tests
- Docker/Compose changes: `docker compose config`
- behavior changes: update README, validation report, `.http`/Postman, migration notes, and expected responses

อย่าใช้คำว่า "เสร็จแล้ว" ถ้ายังไม่ได้ตรวจด้วยหลักฐานที่เหมาะกับงานนั้น
