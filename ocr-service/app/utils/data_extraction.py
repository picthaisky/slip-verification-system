import re
from typing import Optional, Dict, List
from datetime import datetime


class ThaiSlipPatterns:
    """Regular expression patterns for Thai bank slip data extraction"""
    
    # Thai banks information
    THAI_BANKS = {
        "bangkok": {"name": "Bangkok Bank", "code": "BBL", "thai": "ธนาคารกรุงเทพ"},
        "kasikorn": {"name": "Kasikorn Bank", "code": "KBANK", "thai": "ธนาคารกสิกรไทย"},
        "scb": {"name": "Siam Commercial Bank", "code": "SCB", "thai": "ธนาคารไทยพาณิชย์"},
        "krungthai": {"name": "Krungthai Bank", "code": "KTB", "thai": "ธนาคารกรุงไทย"},
        "tmb": {"name": "TMB Thanachart Bank", "code": "TTB", "thai": "ธนาคารทหารไทยธนชาต"},
        "krungsri": {"name": "Krungsri Bank", "code": "BAY", "thai": "ธนาคารกรุงศรีอยุธยา"},
        "gsb": {"name": "Government Savings Bank", "code": "GSB", "thai": "ธนาคารออมสิน"},
        "baac": {"name": "Bank for Agriculture", "code": "BAAC", "thai": "ธ.ก.ส."},
        "uob": {"name": "UOB Thailand", "code": "UOB", "thai": "ธนาคารยูโอบี"},
        "cimb": {"name": "CIMB Thai Bank", "code": "CIMB", "thai": "ธนาคารซีไอเอ็มบี"},
        "promptpay": {"name": "PromptPay", "code": "PROMPTPAY", "thai": "พร้อมเพย์"}
    }
    
    # Amount patterns (supports Thai and English)
    AMOUNT_PATTERNS = [
        r'(?:จำนวนเงิน|amount|total|รวม)\s*:?\s*฿?\s*([\d,]+\.?\d*)\s*(?:บาท|thb|baht)?',
        r'฿\s*([\d,]+\.?\d*)',
        r'([\d,]+\.?\d*)\s*(?:บาท|baht|thb)',
        r'(?:total|amount)\s*:?\s*([\d,]+\.?\d*)',
    ]
    
    # Date patterns (DD/MM/YYYY, DD-MM-YYYY, etc.)
    DATE_PATTERNS = [
        r'(\d{1,2}[/-]\d{1,2}[/-]\d{4})',
        r'(\d{1,2}\s+(?:ม\.ค\.|ก\.พ\.|มี\.ค\.|เม\.ย\.|พ\.ค\.|มิ\.ย\.|ก\.ค\.|ส\.ค\.|ก\.ย\.|ต\.ค\.|พ\.ย\.|ธ\.ค\.)\s+\d{4})',
        r'(\d{4}-\d{2}-\d{2})',
    ]
    
    # Time patterns (HH:MM:SS, HH:MM)
    TIME_PATTERNS = [
        r'(\d{1,2}:\d{2}:\d{2})',
        r'(\d{1,2}:\d{2})',
        r'(?:เวลา|time)\s*:?\s*(\d{1,2}:\d{2}(?::\d{2})?)',
    ]
    
    # Reference number patterns
    REFERENCE_PATTERNS = [
        r'(?:เลขที่อ้างอิง|ref\.?|reference)\s*:?\s*([A-Z0-9]{8,})',
        r'(?:transaction\s*id|trans\s*id)\s*:?\s*([A-Z0-9]{8,})',
        r'([A-Z0-9]{12,20})',  # Generic alphanumeric reference
    ]
    
    # Account number patterns
    ACCOUNT_PATTERNS = [
        r'(\d{3}-\d{1}-\d{5}-\d{1})',  # Format: XXX-X-XXXXX-X
        r'(\d{10,12})',  # 10-12 digit account numbers
        r'(?:เลขที่บัญชี|account\s*no\.?)\s*:?\s*([\d-]+)',
    ]
    
    # PromptPay patterns (phone number or ID card)
    PROMPTPAY_PATTERNS = [
        r'พร้อมเพย์|promptpay',
        r'0\d{9}',  # Phone number
        r'\d{13}',  # ID card number
    ]
    
    @classmethod
    def detect_bank(cls, text: str) -> Optional[Dict[str, str]]:
        """Detect bank from text"""
        text_lower = text.lower()
        
        for bank_key, bank_info in cls.THAI_BANKS.items():
            # Check English name
            if bank_info["name"].lower() in text_lower:
                return bank_info
            # Check bank code
            if bank_info["code"].lower() in text_lower:
                return bank_info
            # Check Thai name
            if bank_info["thai"] in text:
                return bank_info
        
        return None
    
    @classmethod
    def extract_amount(cls, text: str) -> Optional[float]:
        """Extract amount from text"""
        for pattern in cls.AMOUNT_PATTERNS:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                amount_str = match.group(1).replace(',', '')
                try:
                    return float(amount_str)
                except ValueError:
                    continue
        return None
    
    @classmethod
    def extract_date(cls, text: str) -> Optional[str]:
        """Extract date from text"""
        for pattern in cls.DATE_PATTERNS:
            match = re.search(pattern, text)
            if match:
                return match.group(1)
        return None
    
    @classmethod
    def extract_time(cls, text: str) -> Optional[str]:
        """Extract time from text"""
        for pattern in cls.TIME_PATTERNS:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return match.group(1)
        return None
    
    @classmethod
    def extract_reference(cls, text: str) -> Optional[str]:
        """Extract reference number from text"""
        for pattern in cls.REFERENCE_PATTERNS:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return match.group(1)
        return None
    
    @classmethod
    def extract_accounts(cls, text: str) -> List[str]:
        """Extract account numbers from text"""
        accounts = []
        for pattern in cls.ACCOUNT_PATTERNS:
            matches = re.findall(pattern, text)
            accounts.extend(matches)
        return list(set(accounts))  # Remove duplicates
    
    @classmethod
    def is_promptpay(cls, text: str) -> bool:
        """Check if transaction is PromptPay"""
        for pattern in cls.PROMPTPAY_PATTERNS:
            if re.search(pattern, text, re.IGNORECASE):
                return True
        return False


class DataExtractor:
    """Extract structured data from OCR text"""
    
    @staticmethod
    def extract_all(raw_text: str) -> Dict:
        """Extract all possible data from OCR text"""
        # Detect bank
        bank_info = ThaiSlipPatterns.detect_bank(raw_text)
        bank = None
        if bank_info:
            bank = {
                "name": bank_info["name"],
                "code": bank_info["code"]
            }
        
        # Extract amount
        amount = ThaiSlipPatterns.extract_amount(raw_text)
        
        # Extract date and time
        transaction_date = ThaiSlipPatterns.extract_date(raw_text)
        transaction_time = ThaiSlipPatterns.extract_time(raw_text)
        
        # Extract reference number
        reference_number = ThaiSlipPatterns.extract_reference(raw_text)
        
        # Extract account numbers
        accounts = ThaiSlipPatterns.extract_accounts(raw_text)
        sender_account = accounts[0] if len(accounts) > 0 else None
        receiver_account = accounts[1] if len(accounts) > 1 else None
        
        # Check if PromptPay
        if ThaiSlipPatterns.is_promptpay(raw_text):
            if not bank:
                bank = {
                    "name": "PromptPay",
                    "code": "PROMPTPAY"
                }
        
        return {
            "amount": amount,
            "transaction_date": transaction_date,
            "transaction_time": transaction_time,
            "reference_number": reference_number,
            "bank": bank,
            "sender_account": sender_account,
            "receiver_account": receiver_account,
            "sender_name": None,  # Would need more advanced NLP
            "receiver_name": None  # Would need more advanced NLP
        }
