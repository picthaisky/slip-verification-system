import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Slip Upload Flow
 * Tests the complete user journey from login to slip verification
 */

test.describe('Slip Upload Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application
    await page.goto('http://localhost:4200');
  });

  test('should complete full upload process', async ({ page }) => {
    // Step 1: Login
    await page.goto('http://localhost:4200/login');
    await page.fill('input[name="email"]', 'testuser@example.com');
    await page.fill('input[name="password"]', 'Test@123456');
    await page.click('button[type="submit"]');
    
    // Wait for navigation to complete
    await page.waitForURL('**/dashboard');
    await expect(page).toHaveURL(/.*dashboard/);

    // Step 2: Navigate to slip upload
    await page.click('text=Upload Slip');
    await page.waitForURL('**/slips/upload');

    // Step 3: Fill order ID
    await page.fill('input[name="orderId"]', '12345678-1234-1234-1234-123456789012');

    // Step 4: Upload file
    const fileInput = page.locator('input[type="file"]');
    await fileInput.setInputFiles({
      name: 'test-slip.jpg',
      mimeType: 'image/jpeg',
      buffer: Buffer.from('fake-image-content')
    });

    // Step 5: Wait for preview to load
    await page.waitForSelector('.image-preview', { state: 'visible' });

    // Step 6: Submit the form
    await page.click('button[type="submit"]');

    // Step 7: Wait for success message
    await page.waitForSelector('.success-message', { timeout: 10000 });
    await expect(page.locator('.success-message')).toContainText('uploaded successfully');

    // Step 8: Verify redirect to slip details
    await page.waitForURL('**/slips/*');
    await expect(page.locator('h1')).toContainText('Slip Details');
  });

  test('should show error on invalid file', async ({ page }) => {
    // Login first
    await page.goto('http://localhost:4200/login');
    await page.fill('input[name="email"]', 'testuser@example.com');
    await page.fill('input[name="password"]', 'Test@123456');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard');

    // Navigate to upload page
    await page.goto('http://localhost:4200/slips/upload');

    // Fill order ID
    await page.fill('input[name="orderId"]', '12345678-1234-1234-1234-123456789012');

    // Try to upload invalid file (PDF)
    const fileInput = page.locator('input[type="file"]');
    await fileInput.setInputFiles({
      name: 'document.pdf',
      mimeType: 'application/pdf',
      buffer: Buffer.from('fake-pdf-content')
    });

    // Verify error message is shown
    await expect(page.locator('.error-message')).toBeVisible();
    await expect(page.locator('.error-message')).toContainText('Invalid file type');
  });

  test('should validate required fields', async ({ page }) => {
    // Login
    await page.goto('http://localhost:4200/login');
    await page.fill('input[name="email"]', 'testuser@example.com');
    await page.fill('input[name="password"]', 'Test@123456');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard');

    // Navigate to upload page
    await page.goto('http://localhost:4200/slips/upload');

    // Try to submit without filling required fields
    await page.click('button[type="submit"]');

    // Verify validation errors
    await expect(page.locator('.field-error')).toHaveCount(2); // Order ID and File
  });
});

test.describe('Admin Verification Flow', () => {
  test('should allow admin to verify slip', async ({ page }) => {
    // Login as admin
    await page.goto('http://localhost:4200/login');
    await page.fill('input[name="email"]', 'admin@example.com');
    await page.fill('input[name="password"]', 'Admin@123456');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard');

    // Navigate to pending slips
    await page.goto('http://localhost:4200/admin/slips?status=pending');

    // Click on first slip
    await page.click('.slip-item:first-child');

    // Wait for slip details
    await page.waitForSelector('.slip-details');

    // Approve the slip
    await page.click('button:has-text("Approve")');

    // Add verification notes
    await page.fill('textarea[name="notes"]', 'Payment verified');

    // Confirm approval
    await page.click('button:has-text("Confirm")');

    // Verify success message
    await expect(page.locator('.success-message')).toContainText('Slip approved');
  });
});
