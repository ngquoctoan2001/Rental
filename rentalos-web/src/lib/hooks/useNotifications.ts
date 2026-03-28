'use client';

import { useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/lib/stores/authStore';
import { useNotificationStore } from '@/lib/stores/notificationStore';
import { notificationsApi } from '@/lib/api';
import { SIGNALR_URL } from '@/lib/api/client';

export function useNotifications() {
  const { accessToken } = useAuthStore();
  const { addNotification, setNotifications } = useNotificationStore();

  // Load initial notifications from API
  useEffect(() => {
    if (!accessToken) return;
    notificationsApi.list().then(res => {
      const data = Array.isArray(res.data) ? res.data : res.data?.items ?? [];
      setNotifications(data);
    }).catch(() => {});
  }, [accessToken, setNotifications]);

  useEffect(() => {
    if (!accessToken) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_URL, {
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
