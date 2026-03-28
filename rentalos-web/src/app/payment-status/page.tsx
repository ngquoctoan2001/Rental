'use client';

import { Suspense } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { CheckCircle2, XCircle } from 'lucide-react';

function PaymentStatusPageContent() {
  const searchParams = useSearchParams();
  const status = searchParams.get('status');
  const invoiceCode = searchParams.get('invoiceCode');
  const isSuccess = status === 'success';

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-6">
      <div className="max-w-md w-full bg-white rounded-3xl border border-slate-100 shadow-sm p-8 text-center space-y-5">
        <div className="mx-auto w-16 h-16 rounded-full flex items-center justify-center bg-slate-50">
          {isSuccess ? (
            <CheckCircle2 className="w-9 h-9 text-emerald-600" />
          ) : (
            <XCircle className="w-9 h-9 text-rose-600" />
          )}
        </div>

        <h1 className="text-2xl font-black text-slate-900">
          {isSuccess ? 'Thanh toán thành công' : 'Thanh toán thất bại'}
        </h1>

        <p className="text-sm font-medium text-slate-500">
          {isSuccess
            ? 'Hệ thống đã xác nhận giao dịch. Bạn có thể đóng trang này hoặc quay lại liên kết thanh toán.'
            : 'Giao dịch chưa thành công. Vui lòng thử lại bằng liên kết thanh toán hoặc liên hệ quản lý.'}
        </p>

        {invoiceCode && (
          <div className="py-2 px-3 rounded-xl bg-slate-50 text-slate-700 text-sm font-bold">
            Mã hóa đơn: {invoiceCode}
          </div>
        )}

        <Link
          href="/login"
          className="inline-flex items-center justify-center w-full py-3 rounded-2xl bg-slate-900 text-white font-bold hover:bg-slate-800 transition-colors"
        >
          Về trang đăng nhập
        </Link>
      </div>
    </div>
  );
}

export default function PaymentStatusPage() {
  return (
    <Suspense fallback={null}>
      <PaymentStatusPageContent />
    </Suspense>
  );
}
