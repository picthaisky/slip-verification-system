#!/usr/bin/env python3
"""
Example script demonstrating OCR service API usage
"""

import requests
import time
import sys
from pathlib import Path


def process_slip(image_path: str, api_url: str = "http://localhost:8000"):
    """
    Process a slip image using the OCR service
    
    Args:
        image_path: Path to the slip image
        api_url: OCR service base URL
    """
    print(f"Processing slip: {image_path}")
    print(f"API URL: {api_url}")
    print("-" * 50)
    
    # Check if file exists
    if not Path(image_path).exists():
        print(f"Error: File not found: {image_path}")
        return
    
    # Upload and process image
    print("1. Uploading and processing image...")
    with open(image_path, "rb") as f:
        files = {"file": f}
        data = {"preprocess": True}
        
        try:
            response = requests.post(
                f"{api_url}/api/ocr/process",
                files=files,
                data=data,
                timeout=30
            )
            response.raise_for_status()
            result = response.json()
            job_id = result["job_id"]
            print(f"   ✓ Job created: {job_id}")
            print(f"   Status: {result['status']}")
        except Exception as e:
            print(f"   ✗ Error: {e}")
            return
    
    # Check status
    print(f"\n2. Checking status...")
    try:
        response = requests.get(f"{api_url}/api/ocr/status/{job_id}", timeout=10)
        response.raise_for_status()
        status = response.json()
        print(f"   Status: {status['status']}")
        print(f"   Progress: {status.get('progress', 0):.1f}%")
    except Exception as e:
        print(f"   ✗ Error: {e}")
    
    # Get result
    print(f"\n3. Getting OCR result...")
    time.sleep(1)  # Brief delay for processing
    
    try:
        response = requests.get(f"{api_url}/api/ocr/result/{job_id}", timeout=10)
        response.raise_for_status()
        result = response.json()
        
        print(f"\n{'='*50}")
        print("OCR RESULT")
        print(f"{'='*50}")
        
        # Display extracted data
        if result.get("extracted_data"):
            data = result["extracted_data"]
            
            print(f"\nExtracted Data:")
            print(f"  Amount:           {data.get('amount', 'N/A')} THB")
            print(f"  Date:             {data.get('transaction_date', 'N/A')}")
            print(f"  Time:             {data.get('transaction_time', 'N/A')}")
            print(f"  Reference:        {data.get('reference_number', 'N/A')}")
            
            if data.get('bank'):
                print(f"  Bank:             {data['bank'].get('name', 'N/A')} ({data['bank'].get('code', 'N/A')})")
            
            print(f"  Sender Account:   {data.get('sender_account', 'N/A')}")
            print(f"  Receiver Account: {data.get('receiver_account', 'N/A')}")
        
        # Display metadata
        print(f"\nMetadata:")
        print(f"  Confidence:       {result.get('confidence', 0):.2%}")
        print(f"  OCR Engine:       {result.get('ocr_engine', 'N/A')}")
        print(f"  Processing Time:  {result.get('processing_time', 0):.2f}s")
        print(f"  Status:           {result.get('status', 'N/A')}")
        
        # Display raw text (truncated)
        if result.get('raw_text'):
            raw_text = result['raw_text']
            print(f"\nRaw OCR Text (first 200 chars):")
            print(f"  {raw_text[:200]}...")
        
        print(f"\n{'='*50}\n")
        
    except Exception as e:
        print(f"   ✗ Error: {e}")


def batch_process(image_paths: list, api_url: str = "http://localhost:8000"):
    """
    Process multiple slip images in batch
    
    Args:
        image_paths: List of image paths
        api_url: OCR service base URL
    """
    print(f"Batch processing {len(image_paths)} images")
    print(f"API URL: {api_url}")
    print("-" * 50)
    
    # Prepare files
    files = []
    for path in image_paths:
        if Path(path).exists():
            files.append(("files", open(path, "rb")))
        else:
            print(f"Warning: File not found: {path}")
    
    if not files:
        print("Error: No valid files to process")
        return
    
    # Upload batch
    print(f"1. Uploading {len(files)} images...")
    try:
        data = {"preprocess": True}
        response = requests.post(
            f"{api_url}/api/ocr/batch",
            files=files,
            data=data,
            timeout=60
        )
        response.raise_for_status()
        result = response.json()
        
        print(f"   ✓ Batch created: {result['batch_id']}")
        print(f"   Total jobs: {result['total']}")
        print(f"   Job IDs: {', '.join(result['job_ids'][:3])}...")
        
        # Close files
        for _, f in files:
            f.close()
        
        # Check results for each job
        print(f"\n2. Retrieving results...")
        for idx, job_id in enumerate(result['job_ids'], 1):
            print(f"\n   Image {idx}/{result['total']}: {job_id}")
            try:
                resp = requests.get(f"{api_url}/api/ocr/result/{job_id}", timeout=10)
                resp.raise_for_status()
                job_result = resp.json()
                
                print(f"      Status: {job_result.get('status')}")
                print(f"      Confidence: {job_result.get('confidence', 0):.2%}")
                
                if job_result.get('extracted_data', {}).get('amount'):
                    print(f"      Amount: {job_result['extracted_data']['amount']} THB")
                
            except Exception as e:
                print(f"      ✗ Error: {e}")
    
    except Exception as e:
        print(f"   ✗ Error: {e}")
        # Close files on error
        for _, f in files:
            f.close()


def check_health(api_url: str = "http://localhost:8000"):
    """Check OCR service health"""
    print(f"Checking OCR service health...")
    print(f"API URL: {api_url}")
    print("-" * 50)
    
    try:
        response = requests.get(f"{api_url}/health", timeout=5)
        response.raise_for_status()
        health = response.json()
        
        print(f"Status:           {health.get('status', 'unknown')}")
        print(f"Version:          {health.get('version', 'unknown')}")
        print(f"Redis Connected:  {health.get('redis_connected', False)}")
        print(f"Timestamp:        {health.get('timestamp', 'unknown')}")
        
        if health.get('status') == 'healthy':
            print("\n✓ Service is healthy")
        else:
            print("\n⚠ Service is degraded")
        
    except Exception as e:
        print(f"✗ Service is unavailable: {e}")


if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(description="OCR Service API Example")
    parser.add_argument("--url", default="http://localhost:8000", help="OCR service URL")
    parser.add_argument("--health", action="store_true", help="Check service health")
    parser.add_argument("--image", help="Process single image")
    parser.add_argument("--batch", nargs="+", help="Process multiple images")
    
    args = parser.parse_args()
    
    if args.health:
        check_health(args.url)
    elif args.image:
        process_slip(args.image, args.url)
    elif args.batch:
        batch_process(args.batch, args.url)
    else:
        parser.print_help()
        print("\nExamples:")
        print("  python example_usage.py --health")
        print("  python example_usage.py --image slip.jpg")
        print("  python example_usage.py --batch slip1.jpg slip2.jpg slip3.jpg")
