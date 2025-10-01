/**
 * K6 Spike Test - Slip Verification System
 * 
 * Test Scenario: Spike Test - Test system recovery from sudden traffic spikes
 * 
 * Usage:
 *   k6 run spike-test.js
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '1m', target: 20 },   // Normal load
    { duration: '30s', target: 500 }, // Sudden spike!
    { duration: '2m', target: 500 },  // Maintain spike
    { duration: '1m', target: 20 },   // Return to normal
    { duration: '1m', target: 0 },    // Scale down
  ],
  thresholds: {
    http_req_duration: ['p(99)<3000'], // 99% of requests should be below 3s
    http_req_failed: ['rate<0.1'],     // Error rate should be below 10%
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000/api/v1';

export default function() {
  // Test health endpoint during spike
  const res = http.get(`${BASE_URL.replace('/api/v1', '')}/health`);
  
  check(res, {
    'health check status is 200 or 503': (r) => r.status === 200 || r.status === 503,
    'health check responds within 3s': (r) => r.timings.duration < 3000,
  }) || errorRate.add(1);

  sleep(0.1); // Minimal sleep to maximize spike effect
}

export function teardown(data) {
  console.log('Spike test completed');
}
