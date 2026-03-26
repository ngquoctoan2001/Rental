'use client';

import { useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/lib/stores/authStore';
import { useNotificationStore } from '@/lib/stores/notificationStore';

export function useNotifications() {
  const { accessToken } = useAuthStore();
  const { addNotification } = useNotificationStore();

  useEffect(() => {
    if (!accessToken) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.NEXT_PUBLIC_SIGNALR_URL || 'http://localhost:5000/notifications'}`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connection.on('NewNotification', (notification) => {
      addNotification(notification);
    });

    // Ví dụ: Lắng nghe sự kiện thanh toán thành công để cập nhật UI ngay lập tức
    connection.on('InvoicePaid', (invoiceId) => {
      addNotification({
        id: Math.random().toString(),
        title: 'Thanh toán thành công',
        message: `Hóa đơn ${invoiceId} đã được thanh toán.`,
        type: 'success',
        isRead: false,
        createdAt: new Date().toISOString(),
      });
    });

    connection.start().catch((err) => console.error('SignalR Connection Error: ', err));

    return () => {
      connection.stop();
    };
  }, [accessToken, addNotification]);
}
