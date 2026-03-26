public static class PromptBuilder
{
  public static string Build(StatsDto stats, List<DailyProductStatDto> dailyStats)
  {
    var productStatsText = string.Join("\n", dailyStats.Select(x =>
        $"- {x.ProductName}: SL={x.TotalQuantity}, DoanhThu={x.TotalRevenue}, LoiNhuan={x.TotalProfit}"
    ));

    return $@"
Bạn là chuyên gia quản lý cửa hàng tiện lợi tại Hà Nội, Việt Nam.

Mục tiêu:
- Tối ưu doanh thu
- Tối ưu lợi nhuận
- Tránh tồn kho cao
- Đưa ra quyết định giống người quản lý thực tế

========================
DỮ LIỆU TỔNG QUAN (7 NGÀY)
========================

- Tổng doanh thu: {stats.TotalRevenue} VND
- Tổng lợi nhuận: {stats.TotalProfit} VND

- Top sản phẩm bán chạy:
{string.Join(", ", stats.TopProducts)}

- Sản phẩm tồn kho cao:
{string.Join(", ", stats.HighStockProducts)}

========================
CHI TIẾT THEO SẢN PHẨM
========================

{productStatsText}

========================
YÊU CẦU PHÂN TÍCH
========================

1. Đánh giá xu hướng bán hàng:
   - Nhóm nào bán tốt
   - Nhóm nào đang chậm
   - Có dấu hiệu tồn kho nguy hiểm không

2. Đưa ra đề xuất nhập hàng:
   - NÊN nhập thêm (giải thích ngắn)
   - KHÔNG nên nhập / hạn chế nhập (giải thích ngắn)

3. Insight quan trọng:
   - Nhận xét như một quản lý cửa hàng thực tế
   - Có thể đề cập mùa vụ, hành vi khách hàng tại Hà Nội

========================
FORMAT OUTPUT (BẮT BUỘC)
========================

KHÔNG dùng markdown (** hoặc ###)

Chỉ dùng format plain text như sau:

NÊN NHẬP:
- ...

HẠN CHẾ:
- ...

INSIGHT:
- ...

Viết ngắn gọn, rõ ràng, thực tế.
";
  }
}
