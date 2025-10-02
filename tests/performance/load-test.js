/**
 * K6 Load Test - Slip Verification System
 * 
 * Test Scenario: Load Test - Simulate normal load
 * 
 * Usage:
 *   k6 run load-test.js
 * 
 * With options:
 *   k6 run --vus 100 --duration 5m load-test.js
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Test configuration
export const options = {
  stages: [
    { duration: '2m', target: 50 },  // Ramp up to 50 users over 2 minutes
    { duration: '5m', target: 100 }, // Stay at 100 users for 5 minutes
    { duration: '2m', target: 0 },   // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    http_req_failed: ['rate<0.01'],   // Error rate should be below 1%
    errors: ['rate<0.1'],              // Custom error rate should be below 10%
  },
};

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000/api/v1';
const AUTH_TOKEN = __ENV.AUTH_TOKEN || 'your-test-token-here';

// Test data
const testUser = {
  email: `testuser-${Date.now()}@example.com`,
  password: 'Test@123456',
  name: 'Load Test User'
};

export function setup() {
  // Register and login to get token
  const registerRes = http.post(`${BASE_URL}/auth/register`, JSON.stringify({
    email: testUser.email,
    password: testUser.password,
    confirmPassword: testUser.password,
    name: testUser.name,
    role: 'User'
  }), {
    headers: { 'Content-Type': 'application/json' }
  });

  if (registerRes.status === 200 || registerRes.status === 201) {
    const loginRes = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
      email: testUser.email,
      password: testUser.password
    }), {
      headers: { 'Content-Type': 'application/json' }
    });

    if (loginRes.status === 200) {
      return { token: loginRes.json('token') };
    }
  }

  // Fallback to provided token
  return { token: AUTH_TOKEN };
}

export default function(data) {
  const params = {
    headers: {
      'Authorization': `Bearer ${data.token}`,
      'Content-Type': 'application/json',
    },
  };

  // Test 1: Get user profile
  let res = http.get(`${BASE_URL}/auth/me`, params);
  check(res, {
    'auth/me status is 200': (r) => r.status === 200,
    'auth/me response time < 200ms': (r) => r.timings.duration < 200,
  }) || errorRate.add(1);

  sleep(1);

  // Test 2: List orders
  res = http.get(`${BASE_URL}/orders?page=1&pageSize=10`, params);
  check(res, {
    'orders list status is 200': (r) => r.status === 200,
    'orders list response time < 500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);

  sleep(1);

  // Test 3: Create order
  const orderData = {
    orderNumber: `ORD-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
    amount: Math.floor(Math.random() * 10000) + 100,
    description: 'Load test order',
    notes: 'Performance testing'
  };

  res = http.post(`${BASE_URL}/orders`, JSON.stringify(orderData), params);
  check(res, {
    'create order status is 200 or 201': (r) => r.status === 200 || r.status === 201,
    'create order response time < 1000ms': (r) => r.timings.duration < 1000,
  }) || errorRate.add(1);

  if (res.status === 200 || res.status === 201) {
    const orderId = res.json('id');
    
    sleep(1);

    // Test 4: Get order details
    res = http.get(`${BASE_URL}/orders/${orderId}`, params);
    check(res, {
      'get order status is 200': (r) => r.status === 200,
      'get order response time < 200ms': (r) => r.timings.duration < 200,
    }) || errorRate.add(1);
  }

  sleep(2);

  // Test 5: List slips
  res = http.get(`${BASE_URL}/slips?page=1&pageSize=10`, params);
  check(res, {
    'slips list status is 200': (r) => r.status === 200,
    'slips list response time < 500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);

  sleep(1);

  // Test 6: Health check (no auth required)
  res = http.get(`${BASE_URL.replace('/api/v1', '')}/health`);
  check(res, {
    'health check status is 200': (r) => r.status === 200,
    'health check response time < 100ms': (r) => r.timings.duration < 100,
  }) || errorRate.add(1);

  sleep(1);
}

export function teardown(data) {
  // Cleanup if needed
  console.log('Load test completed');
}
