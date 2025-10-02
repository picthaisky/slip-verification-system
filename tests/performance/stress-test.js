/**
 * K6 Stress Test - Slip Verification System
 * 
 * Test Scenario: Stress Test - Test system under heavy load
 * 
 * Usage:
 *   k6 run stress-test.js
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '2m', target: 100 },  // Ramp up to 100 users
    { duration: '5m', target: 200 },  // Increase to 200 users
    { duration: '5m', target: 300 },  // Stress level: 300 users
    { duration: '5m', target: 400 },  // Beyond normal capacity: 400 users
    { duration: '10m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'], // 95% of requests should be below 2s under stress
    http_req_failed: ['rate<0.05'],    // Error rate should be below 5% under stress
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000/api/v1';

export function setup() {
  console.log('Starting stress test...');
  return {};
}

export default function() {
  // Simulate high load scenarios
  
  // Test 1: Multiple concurrent health checks
  const responses = http.batch([
    ['GET', `${BASE_URL.replace('/api/v1', '')}/health`],
    ['GET', `${BASE_URL.replace('/api/v1', '')}/health`],
    ['GET', `${BASE_URL.replace('/api/v1', '')}/health`],
  ]);

  responses.forEach((res, idx) => {
    check(res, {
      [`batch health check ${idx} is 200`]: (r) => r.status === 200,
    }) || errorRate.add(1);
  });

  sleep(0.5);

  // Test 2: Rapid login attempts (stress authentication)
  const loginRes = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
    email: 'testuser@example.com',
    password: 'Test@123456'
  }), {
    headers: { 'Content-Type': 'application/json' }
  });

  check(loginRes, {
    'login under stress succeeds or rate limited': (r) => 
      r.status === 200 || r.status === 429,
  }) || errorRate.add(1);

  sleep(0.5);
}

export function teardown(data) {
  console.log('Stress test completed');
}
