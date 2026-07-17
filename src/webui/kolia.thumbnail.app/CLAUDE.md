# CLAUDE.md

Hướng dẫn dành cho Claude (Claude Code / Claude in chat) khi làm việc trên repo này.

**Trước khi sửa bất kỳ file `.tsx` hoặc `.css` nào, đọc theo đúng thứ tự:**

1. `AGENTS.md` — tổng quan dự án, lệnh build/dev/lint
2. `RULES.md` — quy tắc bắt buộc, đặc biệt là hệ thống theme sáng/tối
3. `.agent/skills/theming.skill.md` — chi tiết kỹ thuật + ví dụ để thêm màu đúng cách

Ba file trên là nguồn quy tắc duy nhất của dự án. Không tạo quy tắc theme/màu sắc
mới khác với những gì đã mô tả ở đó. Nếu RULES.md có vẻ mâu thuẫn với yêu cầu của
người dùng trong phiên làm việc, hãy nêu rõ mâu thuẫn và hỏi lại trước khi phá vỡ
quy tắc.

Sau khi sửa xong, luôn chạy `npm run build` để xác nhận không có lỗi TypeScript/
Tailwind trước khi báo hoàn thành.
