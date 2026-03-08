using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace QuanLyBanHang.Data
{
    public class HoaDon
    {
        public int Id { get; set; }

        public int NhanVienId { get; set; }
       
        public int KhachHangId { get; set; }
       

        public DateTime NgayLap { get; set; }
        public string? GhiChuHoaDon { get; set; }
       

        // Navigation
        public ICollection<HoaDon_ChiTiet> HoaDonChiTiets { get; set; }
        public KhachHang KhachHang { get; set; }
        public NhanVien NhanVien { get; set; }
    }


    public class DanhSachHoaDon
    {
        public int Id { get; set; }

        public int NhanVienId { get; set; }
        public string HoVaTenNhanVien { get; set; }
        public int KhachHangId { get; set; }
        public string HoVaTenKhachHang { get; set; }

        public DateTime NgayLap { get; set; }
        public string? GhiChuHoaDon { get; set; }
        public string? XemChiTiet { get; set; }
        public decimal? TongTienHoaDon { get; set; }

        // Navigation
        public ICollection<HoaDon_ChiTiet> HoaDonChiTiets { get; set; }
        public KhachHang KhachHang { get; set; }
        public NhanVien NhanVien { get; set; }
    }
}
