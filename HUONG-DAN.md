# Chạy game — 3 bước

Mở project bằng **Unity 6000.4.11f1** (đúng phiên bản này, mở bằng bản khác Unity
sẽ đòi nâng cấp).

### 1. Mở scene

Trong cửa sổ **Project**, mở `Assets/Project/Scenes/Test.unity` (bấm đúp).

### 2. Bấm menu **Tower Defense ▸ DỰNG CẢ GAME (bấm 1 lần)**

Chờ vài giây. Cửa sổ **Console** sẽ in `[Tower Defense] Dựng xong.`

Menu này tự làm hết những việc đáng lẽ phải kéo thả tay:

- Tạo asset thông số quái, tháp và 5 đợt
- Tạo prefab `Tower`, `TowerSlot`, `Projectile_Arrow`, `Projectile_Cannonball`
- Gán ảnh và đạn vào từng loại tháp
- Đặt **17 đế tháp** dọc đường đi
- Gắn `GameManager`, `BuildManager`, `AudioManager` và 2 AudioSource
- Dựng toàn bộ giao diện: máu, tiền, số đợt, shop 3 tháp, nút tạm dừng, bảng
  cài đặt, màn thắng/thua, kèm EventSystem

Bấm lại nhiều lần cũng không sao — thứ gì đã có thì bỏ qua, không tạo trùng.

### 3. `Ctrl + S` rồi bấm **Play**

---

## Cách chơi

**Không kéo thả tháp.** Cách xây là:

1. Bấm vào **một trong 3 thẻ tháp** ở đáy màn hình → thẻ sáng viền xanh
2. Rê chuột lên **đế đá** trên bản đồ → đế sáng xanh (đủ tiền) hoặc đỏ (thiếu tiền),
   kèm vòng tròn cho thấy tầm bắn
3. **Bấm vào đế** → tháp mọc lên, tiền bị trừ

Chuột phải để bỏ chọn. Phím `Esc` để tạm dừng.

| Tháp | Giá | Kiểu đánh |
|---|---|---|
| Cung thủ | 50 | Tầm xa, bắn 1 mục tiêu |
| Lính cận chiến | 75 | Tầm ngắn, sát thương cao, trúng ngay |
| Pháo | 100 | Bắn chậm, đạn nổ lan cả cụm |

Bắt đầu có **150 tiền** và **20 máu**. Giết quái ra tiền, quái đi lọt về Base thì
trừ máu. Hết 5 đợt mà còn máu là thắng.

---

## Nếu gặp trục trặc

**Menu "Tower Defense" không hiện** — Unity chưa biên dịch xong. Chờ vòng xoay ở
góc dưới bên phải biến mất. Nếu Console có lỗi đỏ thì chụp gửi lại.

**Bấm Play mà không có quái nào ra** — mở Console xem có dòng
`[EnemySpawner] Chưa gán Wave Data`. Nếu có, chạy lại menu ở bước 2.

**Đợt 5 không thấy boss** — đúng vậy, chưa có prefab boss. Ảnh boss đã có sẵn ở
`Assets/Project/Art/Boss/Boss_Walk-Sheet.png` (11 khung, đã cắt sẵn) nhưng chưa
dựng thành prefab. Game vẫn chạy đủ 5 đợt, chỉ thiếu con boss ở đợt cuối.

**Không nghe thấy tiếng** — chưa gán file âm thanh nào vào `AudioManager`. Đây là
chủ ý: bộ art không kèm âm thanh. Kéo file `.wav`/`.mp3` vào các ô trong
`AudioManager` trên object `GameManager` là có tiếng.

---

## Muốn xem trước mà chưa cài Unity

Thư mục `WebPreview/` có bản chơi thử chạy trên trình duyệt, dùng đúng bộ ảnh,
đúng đường đi và đúng các con số:

```bash
cd WebPreview
python3 -m http.server 8123
```

Rồi mở <http://localhost:8123>. Đây **không phải bài nộp**, chỉ để xem nhịp game.
