import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  stages: [

    { duration: '30s', target: 300 },
    { duration: '1m30s', target: 200 },
    { duration: '20s', target: 0 },

  ],
};

export default function () {
  let url = "https://localhost:5001/api/Auth/login"
  let data = {
    "email": "test@test.com",
    "password": "password"
  }

  http.post(url, data)
}
