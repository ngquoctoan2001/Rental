import type { Metadata, Viewport } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import Providers from "./providers";

const inter = Inter({ subsets: ["latin", "vietnamese"] });

export const metadata: Metadata = {
  title: "RentalOS - Hệ thống quản lý nhà trọ chuyên nghiệp",
  description: "Giải pháp quản lý nhà trọ, căn hộ dịch vụ tích hợp AI và thanh toán tự động.",
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  maximumScale: 1,
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi">
      <body className={`${inter.className} antialiased selection:bg-indigo-100 selection:text-indigo-900`}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
