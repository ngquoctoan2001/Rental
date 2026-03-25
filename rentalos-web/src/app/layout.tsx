import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";

const inter = Inter({ subsets: ["latin", "vietnamese"] });

export const metadata: Metadata = {
  title: "RentalOS - Hệ thống quản lý nhà trọ chuyên nghiệp",
  description: "Giải pháp quản lý nhà trọ, căn hộ dịch vụ tích hợp AI và thanh toán tự động.",
  viewport: "width=device-width, initial-scale=1, maximum-scale=1",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi">
      <body className={`${inter.className} antialiased selection:bg-indigo-100 selection:text-indigo-900`}>
        {children}
      </body>
    </html>
  );
}
 Eskom Root Layout complete. Eskom base architecture ready.
