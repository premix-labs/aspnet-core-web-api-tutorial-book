# Book Documentation Standard

เอกสารนี้เป็นมาตรฐานโครง `docs/internal` สำหรับหนังสือเทคนิคทุกเล่มใน workspace นี้ ใช้เพื่อให้การวางแผน เขียน ตรวจ และ release หนังสือมี pattern เดียวกัน

## Required Structure

ทุกเล่มควรมีโครงขั้นต่ำแบบนี้:

```text
docs/internal/
  README.md
  book-plan.md
  api-contract.md
  final-project-structure.md
  manuscript-status.md
  release-checklist.md
  style-guide.md
  teaching-principles.md
  validation-report.md
  decisions/
  qa/
```

โฟลเดอร์ `decisions/` ใช้เก็บ decision records แบบเลขลำดับ เช่น `0001-tech-stack.md` ส่วน `qa/` ใช้เก็บ checklist สำหรับ browser, accessibility และ security review

## File Roles

- `book-plan.md`: ขอบเขตหนังสือ กลุ่มผู้อ่าน ผลลัพธ์ปลายทาง และลำดับบท
- `api-contract.md`: contract ที่ตัวอย่างหรือ frontend ต้องพึ่งพา เช่น route, DTO, auth และ error shape
- `final-project-structure.md`: โครงสร้าง final project ที่หนังสือพาไปถึง
- `manuscript-status.md`: สถานะบท งานค้าง และความพร้อม release
- `release-checklist.md`: gate ก่อนเผยแพร่หนังสือ
- `style-guide.md`: มาตรฐานภาษา โค้ด ตัวอย่างคำสั่ง และรูปแบบคำอธิบาย
- `teaching-principles.md`: หลักการสอนทีละขั้นและ definition of done ของบทเรียน
- `validation-report.md`: รายงานตรวจ build, examples, ลิงก์, navigation และข้อจำกัดที่เหลือ
- `decisions/`: เหตุผลของ technical choices ที่ไม่ควรกระจายอยู่ในบทเรียน
- `qa/`: checklist สำหรับตรวจคุณภาพก่อนส่งงานหรือ release

## Optional Files

ไฟล์เฉพาะเล่มเพิ่มได้ แต่ต้องตอบว่าไฟล์นั้นใช้ควบคุมคุณภาพหรือการพัฒนาหนังสือจริง ไม่ใช่โน้ตชั่วคราว เช่น frontend book อาจมี `cross-backend-compatibility.md` เพื่อควบคุม compatibility กับ backend หลายภาษา

ถ้าจะเพิ่มไฟล์เฉพาะเล่ม ให้เก็บใน `docs/internal` และอธิบายใน README หน้าแรกของ repo ว่าไฟล์นั้นมีไว้เพื่ออะไร

## Working Order

ก่อนเริ่มเขียนหรือแก้บทเรียน:

1. อ่าน `book-plan.md`
2. อ่าน `teaching-principles.md`
3. อ่าน `style-guide.md`
4. ตรวจ `api-contract.md` และ `final-project-structure.md` ถ้าบทนั้นแตะ behavior หรือ example project
5. อัปเดต `manuscript-status.md` และ `validation-report.md` เมื่อสถานะเปลี่ยน

ก่อน release:

1. ตรวจ `release-checklist.md`
2. ตรวจไฟล์ใน `qa/`
3. รันคำสั่ง build/test ตาม scope
4. อัปเดต `validation-report.md` ด้วยผลตรวจล่าสุด
