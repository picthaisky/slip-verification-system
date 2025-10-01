interface Config {
  API_BASE_URL: string;
  WS_URL: string;
  API_TIMEOUT: number;
}

const config: Config = {
  API_BASE_URL: __DEV__ 
    ? 'http://localhost:5000/api/v1' 
    : 'https://api.slipverification.com/api/v1',
  WS_URL: __DEV__
    ? 'http://localhost:5000'
    : 'https://api.slipverification.com',
  API_TIMEOUT: 30000,
};

export default config;
