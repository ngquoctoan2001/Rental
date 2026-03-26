import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  experimental: {
    // turbo: true // Kích hoạt turbo mode nếu cần trong config (mặc định chạy qua CLI flag)
  },
  images: {
    domains: ['localhost', 'res.cloudinary.com'], // Hỗ trợ Cloudinary nếu dùng sau này
  },
};

export default nextConfig;
