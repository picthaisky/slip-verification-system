import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

// Custom metrics
const orderCreationErrors = new Counter('order_creation_errors');
const slipUploadErrors = new Counter('slip_upload_errors');
const authFailures = new Counter('auth_failures');
const successRate = new Rate('success_rate');
const orderCreationTime = new Trend('order_creation_duration');
const slipUploadTime = new Trend('slip_upload_duration');

// Load test configuration
export const options = {
  stages: [
    { duration: '2m', target: 100 },   // Ramp up to 100 users over 2 minutes
    { duration: '5m', target: 100 },   // Stay at 100 users for 5 minutes
    { duration: '2m', target: 200 },   // Ramp up to 200 users over 2 minutes
    { duration: '5m', target: 200 },   // Stay at 200 users for 5 minutes
    { duration: '2m', target: 500 },   // Ramp up to 500 users over 2 minutes
    { duration: '5m', target: 500 },   // Stay at 500 users for 5 minutes
    { duration: '3m', target: 1000 },  // Ramp up to 1000 users over 3 minutes
    { duration: '10m', target: 1000 }, // Stay at 1000 users for 10 minutes
    { duration: '2m', target: 0 },     // Ramp down to 0 users over 2 minutes
  ],
  thresholds: {
    // 95% of requests should be below 200ms (performance target)
    http_req_duration: ['p(95)<200'],
    // 99% of requests should be below 500ms
    'http_req_duration{expected_response:true}': ['p(99)<500'],
    // Error rate should be below 0.1%
    http_req_failed: ['rate<0.001'],
    // Success rate should be above 99.9%
    success_rate: ['rate>0.999'],
  },
};

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const API_VERSION = 'v1';

// Test data
const testUser = {
  email: __ENV.TEST_USER_EMAIL || 'loadtest@example.com',
  password: __ENV.TEST_USER_PASSWORD || 'Test@123'
};

/**
 * Setup function - runs once at the start
 */
export function setup() {
  console.log(`Starting load test against ${BASE_URL}`);
  console.log(`Test user: ${testUser.email}`);
  return { baseUrl: BASE_URL };
}

/**
 * Main test scenario
 */
export default function (data) {
  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  // Step 1: Login
  const loginPayload = JSON.stringify({
    email: testUser.email,
    password: testUser.password,
  });

  const loginRes = http.post(
    `${data.baseUrl}/api/${API_VERSION}/auth/login`,
    loginPayload,
    params
  );

  const loginSuccess = check(loginRes, {
    'login successful': (r) => r.status === 200,
    'login response time OK': (r) => r.timings.duration < 500,
  });

  if (!loginSuccess) {
    authFailures.add(1);
    successRate.add(0);
    sleep(1);
    return;
  }

  const authToken = loginRes.json('accessToken');
  const authenticatedParams = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authToken}`,
    },
  };

  successRate.add(1);

  // Step 2: Get orders list
  const ordersRes = http.get(
    `${data.baseUrl}/api/${API_VERSION}/orders`,
    authenticatedParams
  );

  check(ordersRes, {
    'orders retrieved': (r) => r.status === 200,
    'orders response time OK': (r) => r.timings.duration < 200,
  });

  successRate.add(ordersRes.status === 200 ? 1 : 0);
  sleep(1);

  // Step 3: Create a new order
  const orderPayload = JSON.stringify({
    amount: Math.random() * 1000 + 100,
    description: `Load test order ${Date.now()}`,
  });

  const createOrderRes = http.post(
    `${data.baseUrl}/api/${API_VERSION}/orders`,
    orderPayload,
    authenticatedParams
  );

  const orderCreated = check(createOrderRes, {
    'order created': (r) => r.status === 200 || r.status === 201,
    'order creation time OK': (r) => r.timings.duration < 200,
  });

  orderCreationTime.add(createOrderRes.timings.duration);

  if (!orderCreated) {
    orderCreationErrors.add(1);
    successRate.add(0);
  } else {
    successRate.add(1);
    
    const orderId = createOrderRes.json('id');
    
    // Step 4: Get order details
    const orderDetailRes = http.get(
      `${data.baseUrl}/api/${API_VERSION}/orders/${orderId}`,
      authenticatedParams
    );

    check(orderDetailRes, {
      'order details retrieved': (r) => r.status === 200,
      'order details response time OK': (r) => r.timings.duration < 200,
    });

    successRate.add(orderDetailRes.status === 200 ? 1 : 0);
  }

  sleep(1);

  // Step 5: Get slip verifications list
  const slipsRes = http.get(
    `${data.baseUrl}/api/${API_VERSION}/slips?page=1&pageSize=10`,
    authenticatedParams
  );

  check(slipsRes, {
    'slips retrieved': (r) => r.status === 200,
    'slips response time OK': (r) => r.timings.duration < 200,
  });

  successRate.add(slipsRes.status === 200 ? 1 : 0);
  sleep(1);

  // Step 6: Get user profile
  const profileRes = http.get(
    `${data.baseUrl}/api/${API_VERSION}/auth/me`,
    authenticatedParams
  );

  check(profileRes, {
    'profile retrieved': (r) => r.status === 200,
    'profile response time OK': (r) => r.timings.duration < 200,
  });

  successRate.add(profileRes.status === 200 ? 1 : 0);
  sleep(2);
}

/**
 * Teardown function - runs once at the end
 */
export function teardown(data) {
  console.log('Load test completed');
  console.log(`Target: ${data.baseUrl}`);
}
