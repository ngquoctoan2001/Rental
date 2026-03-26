import { z } from 'zod';

export const roomSchema = z.object({
  propertyId: z.string().uuid('Vui lòng chọn nhà trọ'),
  roomNumber: z.string().min(1, 'Nhập số phòng').max(20, 'Tối đa 20 ký tự'),
  floor: z.number().int().min(1, 'Tầng tối thiểu là 1').max(50, 'Tối đa tầng 50'),
  areaSqm: z.number().positive('Diện tích phải lớn hơn 0').optional(),
  basePrice: z.number().min(100000, 'Giá thuê tối thiểu 100.000đ'),
  electricityPrice: z.number().min(0, 'Giá điện không được âm').default(3500),
  waterPrice: z.number().min(0, 'Giá nước không được âm').default(15000),
  serviceFee: z.number().min(0, 'Phí dịch vụ không được âm').default(0),
  internetFee: z.number().min(0, 'Phí Internet không được âm').default(0),
  garbageFee: z.number().min(0, 'Phí rác không được âm').default(0),
  amenities: z.array(z.string()).default([]),
  description: z.string().max(500, 'Mô tả tối đa 500 ký tự').optional(),
});

export const roomStatusSchema = z.object({
  status: z.enum(['available', 'occupied', 'maintenance']),
  reason: z.string().max(200, 'Lý do tối đa 200 ký tự').optional(),
});

export type RoomInput = z.infer<typeof roomSchema>;
export type RoomStatusInput = z.infer<typeof roomStatusSchema>;
