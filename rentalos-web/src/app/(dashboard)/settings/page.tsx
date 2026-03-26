'use client';

import { useState } from 'react';
import { 
  CreditCard, Bell, Shield, User, FileText, 
  Wallet, Landmark, Receipt, Sparkles, Check, 
  Zap, Save, ExternalLink, Globe, Smartphone, Settings, Edit3
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

const TABS = [
  { id: 'payment', label: 'Thanh toán', icon: CreditCard },
  { id: 'billing', label: 'Hóa đơn & Nhắc nhở', icon: Receipt },
  { id: 'notification', label: 'Thông báo', icon: Bell },
  { id: 'profile', label: 'Hồ sơ nhà trọ', icon: BuildingLarge },
  { id: 'security', label: 'Bảo mật', icon: Shield },
];

function BuildingLarge(props: any) {
  return (
    <svg 
      {...props}
      xmlns="http://www.w3.org/2000/svg" 
      width="24" height="24" viewBox="0 0 24 24" 
      fill="none" stroke="currentColor" strokeWidth="2" 
      strokeLinecap="round" strokeLinejoin="round"
    >
      <rect width="16" height="20" x="4" y="2" rx="2" ry="2"/>
      <path d="M9 22v-4h6v4"/>
      <path d="M8 6h.01"/>
      <path d="M16 6h.01"/>
      <path d="M8 10h.01"/>
      <path d="M16 10h.01"/>
      <path d="M8 14h.01"/>
      <path d="M16 14h.01"/>
      <path d="M8 18h.01"/>
      <path d="M16 18h.01"/>
    </svg>
  );
}

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState('payment');

  return (
    <div className="flex flex-col lg:flex-row gap-8 pb-20">
      {/* Sidebar Navigation */}
      <aside className="w-full lg:w-[300px] flex flex-col gap-2">
        <h1 className="text-3xl font-black text-slate-900 mb-6 px-4">Cài đặt</h1>
        <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-4 space-y-1">
          {TABS.map(tab => {
            const Icon = tab.icon;
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`w-full flex items-center gap-4 p-4 rounded-xl transition-all font-black text-sm ${
                  activeTab === tab.id 
                  ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-100' 
                  : 'text-slate-500 hover:bg-slate-50'
                }`}
              >
                <Icon className={`w-5 h-5 ${activeTab === tab.id ? 'text-indigo-200' : 'text-slate-400'}`} />
                {tab.label}
              </button>
            );
          })}
        </div>

        <div className="mt-8 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-[2rem] p-8 text-white relative overflow-hidden group">
          <Sparkles className="absolute top-4 right-4 text-indigo-200 animate-pulse" />
          <h4 className="text-lg font-black mb-2 relative z-10">RentalOS Cloud</h4>
          <p className="text-xs font-bold text-indigo-100 mb-6 relative z-10 leading-relaxed">Dữ liệu được bảo mật và tự động sao lưu mỗi 24 giờ.</p>
          <button className="flex items-center gap-2 px-6 py-3 bg-white/20 backdrop-blur-md border border-white/30 rounded-xl text-xs font-black hover:bg-white/30 transition-all">
            Xem gói dịch vụ
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="flex-1 space-y-8">
        <AnimatePresence mode="wait">
          {activeTab === 'payment' && (
            <motion.div 
              id="payment-section"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              className="space-y-8"
            >
              {/* MoMo Section */}
              <section className="bg-white rounded-[2rem] border border-slate-100 shadow-sm overflow-hidden">
                <div className="p-8 border-b border-slate-50 flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <div className="w-14 h-14 rounded-2xl bg-pink-50 flex items-center justify-center text-pink-600 border border-pink-100">
                      <Smartphone className="w-8 h-8" />
                    </div>
                    <div>
                      <h3 className="text-xl font-black text-slate-900 leading-tight">Cổng thanh toán MoMo</h3>
                      <p className="text-xs font-bold text-slate-400 mt-1 uppercase tracking-wider italic">Tự động đối soát và gạch nợ hóa đơn</p>
                    </div>
                  </div>
                  <span className="px-4 py-1.5 bg-emerald-100 text-emerald-700 rounded-full text-[10px] font-black uppercase tracking-widest flex items-center gap-2">
                    <Check className="w-3 h-3" /> Đã kết nối
                  </span>
                </div>
                <div className="p-8 grid grid-cols-1 md:grid-cols-2 gap-8">
                  <div className="space-y-4">
                    <div className="space-y-2">
                      <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Partner Code</label>
                      <input type="text" defaultValue="MOMO_123456" className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-sm font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                    </div>
                    <div className="space-y-2">
                      <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Secret Key</label>
                      <input type="password" defaultValue="••••••••••••" className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-sm font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                    </div>
                  </div>
                  <div className="bg-slate-50 rounded-2xl p-6 flex flex-col justify-center items-center text-center space-y-4 border border-dashed border-slate-200">
                    <p className="text-xs font-bold text-slate-500 italic max-w-[200px]">Hãy đảm bảo bạn đã đăng ký tài khoản Doanh nghiệp tại MoMo.</p>
                    <button className="px-6 py-2.5 bg-white border border-slate-200 text-slate-700 rounded-xl text-xs font-black hover:bg-slate-50 transition-all flex items-center gap-2">
                      Kiểm tra kết nối
                    </button>
                  </div>
                </div>
              </section>

              {/* Bank Account Section */}
              <section className="bg-white rounded-[2rem] border border-slate-100 shadow-sm overflow-hidden">
                <div className="p-8 border-b border-slate-50">
                  <div className="flex items-center gap-4">
                    <div className="w-14 h-14 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 border border-indigo-100">
                      <Landmark className="w-8 h-8" />
                    </div>
                    <div>
                      <h3 className="text-xl font-black text-slate-900 leading-tight">VietQR / Chuyển khoản ngân hàng</h3>
                      <p className="text-xs font-bold text-slate-400 mt-1 uppercase tracking-wider italic">Nhận tiền trực tiếp qua số tài khoản cá nhân</p>
                    </div>
                  </div>
                </div>
                <div className="p-8 grid grid-cols-1 lg:grid-cols-2 gap-12">
                   <div className="space-y-6">
                      <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Ngân hàng</label>
                          <select className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-sm font-bold outline-none focus:bg-white transition-all appearance-none cursor-pointer">
                            <option>Vietcombank</option>
                            <option>Techcombank</option>
                            <option>MB Bank</option>
                          </select>
                        </div>
                        <div className="space-y-2">
                          <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Số tài khoản</label>
                          <input type="text" placeholder="vd: 0987654321" className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-sm font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                        </div>
                      </div>
                      <div className="space-y-2">
                        <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Tên chủ tài khoản (In hoa)</label>
                        <input type="text" placeholder="vd: NGUYEN VAN A" className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-sm font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                      </div>
                      <div className="flex items-center gap-3 p-4 bg-emerald-50 border border-emerald-100 rounded-2xl">
                        <Check className="w-5 h-5 text-emerald-600" />
                        <span className="text-xs font-bold text-emerald-700">Mã QR sẽ tự động được tạo khi khách quét thanh toán.</span>
                      </div>
                   </div>
                   <div className="flex flex-col items-center justify-center space-y-4">
                      <div className="w-48 h-48 bg-slate-50 rounded-[2rem] border-2 border-dashed border-slate-200 flex items-center justify-center relative group">
                        <div className="text-center p-6 bg-white rounded-2xl shadow-sm border border-slate-100 transform -rotate-3 group-hover:rotate-0 transition-transform duration-500">
                           <Globe className="w-12 h-12 text-slate-200 mx-auto mb-2" />
                           <p className="text-[8px] font-bold text-slate-400 uppercase">Preview QR</p>
                        </div>
                      </div>
                      <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">VietQR Standard 2.0</p>
                   </div>
                </div>
              </section>
            </motion.div>
          )}

          {activeTab === 'billing' && (
            <motion.div 
              id="billing-section"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              className="space-y-8"
            >
              <section className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8 space-y-8">
                <div className="flex items-center gap-4 border-b border-slate-50 pb-6">
                  <div className="w-14 h-14 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 border border-indigo-100">
                    <Zap className="w-8 h-8" />
                  </div>
                  <div>
                    <h3 className="text-xl font-black text-slate-900 leading-tight">Tự động hóa hóa đơn</h3>
                    <p className="text-xs font-bold text-slate-400 mt-1 uppercase tracking-wider italic">Hệ thống sẽ thay bạn thực hiện các tác vụ lặp lại</p>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
                   <div className="space-y-8">
                      <div className="flex items-center justify-between group">
                        <div className="space-y-1">
                          <p className="text-sm font-black text-slate-900">Ngày tạo hóa đơn</p>
                          <p className="text-xs font-medium text-slate-400">Hệ thống tự chốt số và gửi thông báo</p>
                        </div>
                        <div className="flex items-center gap-3">
                           <span className="text-xs font-black text-slate-400">Ngày</span>
                           <input type="number" defaultValue={25} className="w-16 px-3 py-2 bg-slate-50 border border-slate-100 rounded-xl text-center font-black text-indigo-600" />
                        </div>
                      </div>

                      <div className="flex items-center justify-between group">
                        <div className="space-y-1">
                          <p className="text-sm font-black text-slate-900">Hạn chót thanh toán</p>
                          <p className="text-xs font-medium text-slate-400">Sau bao nhiêu ngày kể từ lúc tạo</p>
                        </div>
                        <div className="flex items-center gap-3">
                           <input type="number" defaultValue={5} className="w-16 px-3 py-2 bg-slate-50 border border-slate-100 rounded-xl text-center font-black text-indigo-600" />
                           <span className="text-xs font-black text-slate-400">Ngày</span>
                        </div>
                      </div>

                      <div className="flex items-center justify-between group">
                        <div className="space-y-1">
                          <p className="text-sm font-black text-slate-900">Nhắc nợ tự động</p>
                          <p className="text-xs font-medium text-slate-400">Gửi Zalo/SMS/Email nhắc thanh toán</p>
                        </div>
                        <div className="flex items-center gap-3">
                           <span className="text-xs font-black text-slate-400">Trước</span>
                           <input type="number" defaultValue={1} className="w-16 px-3 py-2 bg-slate-50 border border-slate-100 rounded-xl text-center font-black text-indigo-600" />
                           <span className="text-xs font-black text-slate-400">ngày</span>
                        </div>
                      </div>
                   </div>

                   <div className="space-y-8 bg-slate-50/50 rounded-3xl p-8 border border-slate-100">
                      <h4 className="text-[10px] font-black text-slate-400 uppercase tracking-widest flex items-center gap-2">
                        <Settings className="w-3.5 h-3.5" /> Đơn giá mặc định (Toàn hệ thống)
                      </h4>
                      <div className="space-y-4">
                        <div className="flex items-center justify-between">
                          <span className="text-xs font-bold text-slate-600">Giá điện (kWh)</span>
                          <span className="text-sm font-black text-slate-900">3.500đ</span>
                        </div>
                        <div className="flex items-center justify-between">
                          <span className="text-xs font-bold text-slate-600">Giá nước (m3)</span>
                          <span className="text-sm font-black text-slate-900">25.000đ</span>
                        </div>
                        <div className="flex items-center justify-between">
                          <span className="text-xs font-bold text-slate-600">Phí dịch vụ chung</span>
                          <span className="text-sm font-black text-slate-900">150.000đ</span>
                        </div>
                      </div>
                      <button className="w-full py-3 bg-white border border-slate-200 text-indigo-600 rounded-xl text-xs font-black hover:bg-indigo-50 transition-all flex items-center justify-center gap-2">
                        <Edit3 className="w-3.5 h-3.5" /> Chỉnh sửa đơn giá
                      </button>
                   </div>
                </div>
              </section>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Action Bar */}
        <div className="flex items-center justify-between p-6 bg-slate-900 rounded-[2.5rem] shadow-xl text-white sticky bottom-8 z-10 mx-4">
          <div className="flex items-center gap-4">
             <div className="p-2.5 bg-white/10 rounded-xl">
                <Save className="w-6 h-6 text-indigo-400" />
             </div>
             <div>
                <p className="text-sm font-black">Lưu thay đổi</p>
                <p className="text-[10px] font-medium text-slate-400 italic">Cập nhật cấu hình ngay lập tức</p>
             </div>
          </div>
          <button className="px-8 py-3.5 bg-indigo-600 text-white rounded-[1.25rem] font-black hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-500/20 active:scale-95">
             Xác nhận Lưu
          </button>
        </div>
      </main>
    </div>
  );
}
