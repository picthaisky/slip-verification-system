import pytest
from app.utils.data_extraction import ThaiSlipPatterns, DataExtractor


class TestThaiSlipPatterns:
    """Test Thai slip pattern extraction"""
    
    def test_detect_bank_bangkok(self):
        """Test Bangkok Bank detection"""
        text = "ธนาคารกรุงเทพ จำกัด (มหาชน)"
        bank = ThaiSlipPatterns.detect_bank(text)
        assert bank is not None
        assert bank["name"] == "Bangkok Bank"
        assert bank["code"] == "BBL"
    
    def test_detect_bank_kasikorn(self):
        """Test Kasikorn Bank detection"""
        text = "KASIKORN BANK จำกัด (มหาชน)"
        bank = ThaiSlipPatterns.detect_bank(text)
        assert bank is not None
        assert bank["name"] == "Kasikorn Bank"
        assert bank["code"] == "KBANK"
    
    def test_detect_bank_krungsri(self):
        """Test Krungsri Bank detection"""
        text = "ธนาคารกรุงศรีอยุธยา จำกัด"
        bank = ThaiSlipPatterns.detect_bank(text)
        assert bank is not None
        assert bank["name"] == "Krungsri Bank"
        assert bank["code"] == "BAY"
    
    def test_detect_bank_gsb(self):
        """Test Government Savings Bank detection"""
        text = "ธนาคารออมสิน"
        bank = ThaiSlipPatterns.detect_bank(text)
        assert bank is not None
        assert bank["name"] == "Government Savings Bank"
        assert bank["code"] == "GSB"
    
    def test_extract_amount_thai(self):
        """Test amount extraction in Thai"""
        text = "จำนวนเงิน: 1,500.00 บาท"
        amount = ThaiSlipPatterns.extract_amount(text)
        assert amount == 1500.00
    
    def test_extract_amount_english(self):
        """Test amount extraction in English"""
        text = "Amount: ฿2,350.50"
        amount = ThaiSlipPatterns.extract_amount(text)
        assert amount == 2350.50
    
    def test_extract_date_slash_format(self):
        """Test date extraction with slash format"""
        text = "วันที่: 01/10/2024"
        date = ThaiSlipPatterns.extract_date(text)
        assert date == "01/10/2024"
    
    def test_extract_time(self):
        """Test time extraction"""
        text = "เวลา: 14:30:45"
        time = ThaiSlipPatterns.extract_time(text)
        assert time == "14:30:45"
    
    def test_extract_reference(self):
        """Test reference number extraction"""
        text = "เลขที่อ้างอิง: REF123456789"
        ref = ThaiSlipPatterns.extract_reference(text)
        assert ref == "REF123456789"
    
    def test_extract_accounts(self):
        """Test account number extraction"""
        text = "จากบัญชี: 123-4-56789-0 ไปยังบัญชี: 987-6-54321-0"
        accounts = ThaiSlipPatterns.extract_accounts(text)
        assert len(accounts) >= 2
        assert "123-4-56789-0" in accounts
        assert "987-6-54321-0" in accounts
    
    def test_is_promptpay(self):
        """Test PromptPay detection"""
        text = "โอนผ่าน พร้อมเพย์"
        is_pp = ThaiSlipPatterns.is_promptpay(text)
        assert is_pp is True


class TestDataExtractor:
    """Test data extractor"""
    
    def test_extract_all_kasikorn(self):
        """Test extraction from Kasikorn Bank slip"""
        text = """
        ธนาคารกสิกรไทย จำกัด (มหาชน)
        KASIKORN BANK
        
        โอนเงิน
        จำนวนเงิน: 1,500.00 บาท
        วันที่: 01/10/2024
        เวลา: 14:30:45
        เลขที่อ้างอิง: REF123456789
        จากบัญชี: 123-4-56789-0
        ไปยังบัญชี: 987-6-54321-0
        """
        
        result = DataExtractor.extract_all(text)
        
        assert result["amount"] == 1500.00
        assert result["transaction_date"] == "01/10/2024"
        assert result["transaction_time"] == "14:30:45"
        assert result["reference_number"] == "REF123456789"
        assert result["bank"] is not None
        assert result["bank"]["name"] == "Kasikorn Bank"
        # Check that both accounts were extracted (order may vary)
        assert result["sender_account"] in ["123-4-56789-0", "987-6-54321-0"]
        assert result["receiver_account"] in ["123-4-56789-0", "987-6-54321-0"]
    
    def test_extract_all_promptpay(self):
        """Test extraction from PromptPay slip"""
        text = """
        โอนเงินผ่าน พร้อมเพย์
        จำนวนเงิน: ฿500.00
        วันที่: 15/09/2024
        เวลา: 10:15
        """
        
        result = DataExtractor.extract_all(text)
        
        assert result["amount"] == 500.00
        assert result["transaction_date"] == "15/09/2024"
        assert result["transaction_time"] == "10:15"
        assert result["bank"] is not None
        assert result["bank"]["name"] == "PromptPay"
