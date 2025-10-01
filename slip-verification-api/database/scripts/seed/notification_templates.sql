-- =============================================
-- Notification Template Seed Data
-- Description: Pre-configured templates for notifications
-- =============================================

-- Payment Received Templates
INSERT INTO "NotificationTemplates" ("Id", "Code", "Name", "Channel", "Subject", "Body", "Language", "IsActive", "CreatedAt", "IsDeleted")
VALUES 
-- LINE Templates
(gen_random_uuid(), 'payment_received', 'Payment Received - LINE', 0, 'Payment Received', 
'✅ Payment received for Order {{orderNumber}}\n\nAmount: {{amount}} THB\nDate: {{paymentDate}}\n\nThank you for your payment!', 
'en', true, NOW(), false),

(gen_random_uuid(), 'payment_received', 'การชำระเงินได้รับแล้ว - LINE', 0, 'ได้รับการชำระเงิน', 
'✅ ได้รับการชำระเงินสำหรับคำสั่งซื้อ {{orderNumber}}\n\nจำนวนเงิน: {{amount}} บาท\nวันที่: {{paymentDate}}\n\nขอบคุณสำหรับการชำระเงิน!', 
'th', true, NOW(), false),

-- EMAIL Templates
(gen_random_uuid(), 'payment_received', 'Payment Received - Email', 1, 'Payment Received - Order {{orderNumber}}', 
'<h2>Payment Received</h2>
<p>We have received your payment for Order <strong>{{orderNumber}}</strong>.</p>
<ul>
  <li>Amount: <strong>{{amount}} THB</strong></li>
  <li>Payment Date: <strong>{{paymentDate}}</strong></li>
  <li>Transaction ID: <strong>{{transactionId}}</strong></li>
</ul>
<p>Thank you for your payment!</p>', 
'en', true, NOW(), false),

(gen_random_uuid(), 'payment_received', 'ได้รับการชำระเงิน - Email', 1, 'ได้รับการชำระเงิน - คำสั่งซื้อ {{orderNumber}}', 
'<h2>ได้รับการชำระเงิน</h2>
<p>เราได้รับการชำระเงินของคุณสำหรับคำสั่งซื้อ <strong>{{orderNumber}}</strong> แล้ว</p>
<ul>
  <li>จำนวนเงิน: <strong>{{amount}} บาท</strong></li>
  <li>วันที่ชำระ: <strong>{{paymentDate}}</strong></li>
  <li>เลขที่ธุรกรรม: <strong>{{transactionId}}</strong></li>
</ul>
<p>ขอบคุณสำหรับการชำระเงิน!</p>', 
'th', true, NOW(), false),

-- Payment Verified Templates
(gen_random_uuid(), 'payment_verified', 'Payment Verified - LINE', 0, 'Payment Verified', 
'✅ Your payment has been verified!\n\nOrder: {{orderNumber}}\nAmount: {{amount}} THB\nStatus: Verified\n\nYour order is now being processed.', 
'en', true, NOW(), false),

(gen_random_uuid(), 'payment_verified', 'ยืนยันการชำระเงิน - LINE', 0, 'ยืนยันการชำระเงิน', 
'✅ การชำระเงินของคุณได้รับการยืนยันแล้ว!\n\nคำสั่งซื้อ: {{orderNumber}}\nจำนวนเงิน: {{amount}} บาท\nสถานะ: ยืนยันแล้ว\n\nคำสั่งซื้อของคุณกำลังดำเนินการ', 
'th', true, NOW(), false),

-- Payment Failed Templates
(gen_random_uuid(), 'payment_failed', 'Payment Failed - LINE', 0, 'Payment Failed', 
'❌ Payment verification failed\n\nOrder: {{orderNumber}}\nReason: {{reason}}\n\nPlease contact support or try again.', 
'en', true, NOW(), false),

(gen_random_uuid(), 'payment_failed', 'การชำระเงินล้มเหลว - LINE', 0, 'การชำระเงินล้มเหลว', 
'❌ การยืนยันการชำระเงินล้มเหลว\n\nคำสั่งซื้อ: {{orderNumber}}\nเหตุผล: {{reason}}\n\nกรุณาติดต่อฝ่ายสนับสนุนหรือลองอีกครั้ง', 
'th', true, NOW(), false),

-- Order Created Templates
(gen_random_uuid(), 'order_created', 'Order Created - LINE', 0, 'Order Created', 
'🛒 New order created!\n\nOrder Number: {{orderNumber}}\nAmount: {{amount}} THB\nExpected Payment: {{expectedDate}}\n\nPlease complete your payment.', 
'en', true, NOW(), false),

(gen_random_uuid(), 'order_created', 'สร้างคำสั่งซื้อ - LINE', 0, 'สร้างคำสั่งซื้อ', 
'🛒 สร้างคำสั่งซื้อใหม่!\n\nเลขที่คำสั่งซื้อ: {{orderNumber}}\nจำนวนเงิน: {{amount}} บาท\nกำหนดชำระ: {{expectedDate}}\n\nกรุณาชำระเงินให้เสร็จสิ้น', 
'th', true, NOW(), false),

-- Order Cancelled Templates
(gen_random_uuid(), 'order_cancelled', 'Order Cancelled - LINE', 0, 'Order Cancelled', 
'❌ Order cancelled\n\nOrder Number: {{orderNumber}}\nReason: {{reason}}\n\nIf you have any questions, please contact support.', 
'en', true, NOW(), false),

(gen_random_uuid(), 'order_cancelled', 'ยกเลิกคำสั่งซื้อ - LINE', 0, 'ยกเลิกคำสั่งซื้อ', 
'❌ ยกเลิกคำสั่งซื้อ\n\nเลขที่คำสั่งซื้อ: {{orderNumber}}\nเหตุผล: {{reason}}\n\nหากมีคำถาม กรุณาติดต่อฝ่ายสนับสนุน', 
'th', true, NOW(), false),

-- PUSH Notification Templates
(gen_random_uuid(), 'payment_received', 'Payment Received - Push', 2, 'Payment Received', 
'Your payment for order {{orderNumber}} ({{amount}} THB) has been received.', 
'en', true, NOW(), false),

(gen_random_uuid(), 'payment_verified', 'Payment Verified - Push', 2, 'Payment Verified', 
'Your payment for order {{orderNumber}} has been verified and is being processed.', 
'en', true, NOW(), false),

-- SMS Templates (Keep short for SMS)
(gen_random_uuid(), 'payment_received', 'Payment Received - SMS', 3, 'Payment Received', 
'Payment received for order {{orderNumber}}: {{amount}} THB. Thank you!', 
'en', true, NOW(), false),

(gen_random_uuid(), 'payment_verified', 'Payment Verified - SMS', 3, 'Payment Verified', 
'Payment verified for order {{orderNumber}}. Your order is being processed.', 
'en', true, NOW(), false)
ON CONFLICT DO NOTHING;
