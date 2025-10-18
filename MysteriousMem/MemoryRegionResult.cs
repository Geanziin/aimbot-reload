using System;

namespace MysteriousMem
{
	// Token: 0x02000029 RID: 41
	internal struct MemoryRegionResult
	{
		// Token: 0x17000080 RID: 128
		// (get) Token: 0x060001C5 RID: 453 RVA: 0x0000948D File Offset: 0x0000768D
		// (set) Token: 0x060001C6 RID: 454 RVA: 0x00009495 File Offset: 0x00007695
		public UIntPtr CurrentBaseAddress { get; set; }

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x060001C7 RID: 455 RVA: 0x0000949E File Offset: 0x0000769E
		// (set) Token: 0x060001C8 RID: 456 RVA: 0x000094A6 File Offset: 0x000076A6
		public long RegionSize { get; set; }

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x060001C9 RID: 457 RVA: 0x000094AF File Offset: 0x000076AF
		// (set) Token: 0x060001CA RID: 458 RVA: 0x000094B7 File Offset: 0x000076B7
		public UIntPtr RegionBase { get; set; }
	}
}
