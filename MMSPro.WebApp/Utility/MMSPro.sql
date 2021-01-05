use MMSPro
go

--****************************************************************************************
--基础信息表
--****************************************************************************************
create table DepInfo
(
	DepID int identity(1,1) primary key not null, --主键
	DepName nvarchar(50) not null ,
	DepCode nvarchar(50) not null unique, --唯一约束
	InCharge nvarchar(50),
	Contact nvarchar(50),
	Remark nvarchar(200)
)
go
create table EmpInfo
(
	EmpID int identity(1,1) primary key not null, --主键
	Account nvarchar(50) not null unique, --唯一约束
	--PassWord nvarchar(50),
	DepID int not null foreign key references DepInfo(DepID), --外键
	EmpName nvarchar(50),
	Contact nvarchar(50),
	Remark nvarchar(200)
)
go
create table SupplierType
(
	SupplierTypeID int identity(1,1) primary key not null, --主键
	SupplierTypeName nvarchar(50) not null,
	SupplierTypeCode nvarchar(50) not null,
	Remark nvarchar(200)
)
go
create table SupplierInfo
(
	SupplierID int identity(1,1) primary key not null, --主键
	SupplierName nvarchar(50) not null,
	SupplierCode nvarchar(50) not null unique, --唯一约束
	SupplierTypeID int not null foreign key references SupplierType(SupplierTypeID),--外键
	SupplierAddress1 nvarchar(200),
	SupplierAddress2 nvarchar(200),
	SupplierPhone nvarchar(50),
	InCharge nvarchar(50),
	Remark nvarchar(200)
)
go

--所需未存在基础信息，需将位置提到前面
create table ManufacturerType--厂家类型
(
	ManufacturerTypeID int identity(1,1) primary key not null, --主键
	ManufacturerTypeName nvarchar(50) not null,--生产厂商类型名称
	ManufacturerTypeCode nvarchar(50) not null,--生产厂商类型编码
	Remark nvarchar(200)
)
go
create table Manufacturer--生产厂家
(
	ManufacturerID int identity(1,1) primary key not null, --主键
	ManufacturerName nvarchar(50) not null,--厂家名称
	ManufacturerCode nvarchar(50) not null unique, --生产厂商编码，唯一约束
	ManufacturerTypeID int not null foreign key references ManufacturerType(ManufacturerTypeID),--厂家类型，外键
	ManufacturerAddress1 nvarchar(200),--生产厂家地址1
	ManufacturerAddress2 nvarchar(200),--生产厂家地址2
	ManufacturerPhone nvarchar(50),--生产厂家联系电话
	principal nvarchar(50),--负责人
	Remark nvarchar(200)
)
go

create table MaterialType
(
	MaterialTypeID int identity(1,1) primary key not null, --主键
	MaterialTypeName nvarchar(50) not null,
	MaterialTypeCode nvarchar(50) not null unique --唯一约束
)
go
create table MaterialMainType
(
	MaterialMainTypeID int identity(1,1) primary key not null, --主键
	MaterialMainTypeName nvarchar(50) not null,
	MaterialMainTypeCode nvarchar(50) not null unique ,--唯一约束
	MaterialTypeID int not null foreign key references MaterialType(MaterialTypeID) --外键
)
go
create table MaterialChildType
(
	MaterialChildTypeID int identity(1,1) primary key not null, --主键
	MaterialChildTypeName nvarchar(50) not null,
	MaterialChildTypeCode nvarchar(50) not null unique, --唯一约束
	MaterialMainTypeID int not null foreign key references MaterialMainType(MaterialMainTypeID) --外键
)
go
create table MaterialInfo
(
	MaterialID int identity(1,1) primary key not null, --主键
	--MaterialCode  nvarchar(50) not null unique,--唯一约束
	FinanceCode  nvarchar(50),-- 物料编码为"N/A" --此处修改为财务编码 modify by roro
	MaterialName nvarchar(50) not null,
	MaterialchildTypeID int not null foreign key references MaterialChildType(MaterialChildTypeID), --外键
	SpecificationModel  nvarchar(200),--规格型号 --此处修改为规格型号 modify by roro
	Remark nvarchar(200)
)


go
create table BusinessUnitType
(
	BusinessUnitTypeID int identity(1,1) primary key not null, --主键
	BusinessUnitTypeName nvarchar(50) not null,
	BusinessUnitTypeCode nvarchar(50) not null unique,--唯一约束
	Remark nvarchar(200)
)
go
create table BusinessUnitInfo
(
	BusinessUnitID int identity(1,1) primary key not null, --主键
	BusinessUnitName nvarchar(50) not null,
	BusinessUnitCode nvarchar(50) not null unique, --唯一约束
	BusinessUnitTypeID int not null foreign key references BusinessUnitType(BusinessUnitTypeID), --外键
	BusinessUnitAddress1 nvarchar(200),
	BusinessUnitAddress2 nvarchar(200),
	BusinessUnitPhone nvarchar(50),
	InCharger nvarchar(50),
	Remark nvarchar(200)
)

go
create table StorageInfo
(
	StorageID int identity(1,1) primary key not null, --主键
	StorageName nvarchar(50) not null,
	StorageCode nvarchar(50) not null unique, --唯一约束
	EmpID int not null foreign key references EmpInfo(EmpID), --外键
	Remark nvarchar(200)
)
go
create table PileInfo
(
	PileID int identity(1,1) primary key not null, --主键
	PileName nvarchar(50) not null,
	PileCode nvarchar(50) not null unique, --唯一约束
	StorageID int not null foreign key references StorageInfo(StorageID), --外键
	PileSize nvarchar(50),
	Remark nvarchar(200)
)
--鄭平承諾,在垛位與倉庫表數據錄入時驗證特殊符號"|"
go

create table DeliveredTypeInfo
(
	DeliveredTypeID int identity(1,1) primary key not null, --主键
	DeliveredTypeName nvarchar(50) not null,
	DeliveredTypeCode nvarchar(50) not null unique, --唯一约束

)
go
create table ReceivingTypeInfo
(
	ReceivingTypeID int identity(1,1) primary key not null, --主键
	ReceivingTypeName nvarchar(50) not null,
	ReceivingTypeCode nvarchar(50) not null unique, --唯一约束

)
go
--加入初始数据,客户不做维护,此表内容不再做更改
delete  from ReceivingTypeInfo
insert into ReceivingTypeInfo (ReceivingTypeName,ReceivingTypeCode) values('正常入库','01')
insert into ReceivingTypeInfo (ReceivingTypeName,ReceivingTypeCode) values('移入入库','02')

create table ProjectInfo
(
	ProjectID  int identity(1,1) primary key not null, --主键
	Owner int not null foreign key references BusinessUnitInfo(BusinessUnitID),--项目所属业主单位
	ProjectName  nvarchar(50) not null,
	ProjectCode nvarchar(50) not null unique, --唯一约束
	ProjectProperty   nvarchar(50) ,
	Remark nvarchar(200)
)
go
create table RelationProjectBusiness
(
	ProjectID int not null foreign key references ProjectInfo(ProjectID), -- 外键
	BusinessUnitID int not null foreign key references BusinessUnitInfo(BusinessUnitID) Primary Key (ProjectID,BusinessUnitID)--外键
)
go

create table MessageInfo--消息信息表
(
	MessageInfoID int identity(1,1) primary key not null,--主键
	Creater int not null foreign key references EmpInfo(EmpID),--消息发送者
	MessageTitle nvarchar(50),--消息标题
	MessageContent nvarchar(Max),--消息内容
	MessageSource nvarchar(20) not null check(MessageSource in ('回收入库')),--消息来自的流程
	MessageStatus nvarchar(10) not null check(MessageStatus in ('未读','已读')),
	MessageType nvarchar(10) not null check(MessageType in('公共消息','私有消息')),--消息类别
	CreateTime datetime,--创建时间
	TaskID int not null--来自哪个任务，出库任务/入库任务
	
)
go
create table MessageReceiver--消息接收者（一个消息可能有多个接收者）
(
	MessageReceiverID int identity(1,1) primary key not null,--主键
	MessageInfoID int not null foreign key references MessageInfo(MessageInfoID),--消息
	ReceiverID int not null foreign key references EmpInfo(EmpID)--消息接收者
)
go
create table LogInfo
(
	LogID int identity(1,1) primary key not null, --主键
	LogType nvarchar(20) not null check(LogType in ('错误','信息')),
	LogMessage nvarchar(max) not null,
	LogSource nvarchar(50) not null,
	LogUser int not null foreign key references EmpInfo(EmpID),
	LogDateTime datetime not null,
)
go



--****************************************************************************************
--入库流程修改后数据表
--Created By: Adonis
--Date:2010.10.18
--****************************************************************************************

--主要流程
create table BatchOfIndex--批次 
(
	BatchOfIndexID int identity(1,1) primary key not null, --主键
	BatchOfIndexName nvarchar(50) not null--批次名

)
go


create table StorageInMain --入库主表
(
	StorageInID int identity(1,1) primary key not null, --主键
	StorageInCode nvarchar(50) not null,--交货通知单编号
    ReceivingType int foreign key references ReceivingTypeInfo(ReceivingTypeID),--入库类型	    
	Remark nvarchar(200),--备注
	StorageInQualifiedNum nvarchar(50),--质检合格入库编号
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null--创建时间
)
go
create table StorageProduce--生产组信息表
(
	StorageInProduceID int identity(1,1) primary key not null, --主键
	StorageInID int not null foreign key references StorageInMain(StorageInID),--所属入库单,外键
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--物料
	QuantityGentaojian decimal(18,2),--根/台/套/件数量
    QuantityMetre decimal(18,2), --米的数量
    QuantityTon decimal(18,2),--吨的数量
    ExpectedProject int not null foreign key references ProjectInfo(ProjectID),--预期使用项目
	ExpectedTime datetime not null,--预期到库时间
	BatchIndex  nvarchar(50),--批次信息
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)--备注
)
go


create table StorageInMaterials--物资组信息表
(
	StorageInMaterialsID int identity(1,1) primary key not null, --主键
	ProduceID int not null foreign key references StorageProduce(StorageInProduceID),--所属生产组信息表,外键
	RealGentaojian decimal(18,2),--根/台/套/件数量
    RealMetre decimal(18,2), --米的数量
    RealTon decimal(18,2),--吨的数量
	ManufacturerID int not null foreign key references Manufacturer(ManufacturerID),--生产厂家ID,外键
	IsManufacturer nvarchar(10) not null check(IsManufacturer in ('是','否')),--生产厂家信息是否与采购合同一致
	SupplierID int not null foreign key references SupplierInfo(SupplierID),--供应商ID,外键
	Supplier nvarchar(10) not null check(Supplier in ('是','否')),--供应商信息是否与采购合同一致
	Data nvarchar(10) not null check(Data in ('是','否')),--资料是否齐全
	Standard nvarchar(10) not null check(Standard in ('是','否')),--制造标准是否与采购合同一致
	Parts nvarchar(10) not null check(Parts in ('是','否')),--配件是否齐全
	Appearance nvarchar(10) not null check(Appearance in ('是','否')),--外观是否完好
	PileID int not null foreign key references PileInfo(PileID),--所属仓库,所属垛位,外键
	Creator int not null foreign key references EmpInfo(EmpID),--物资管理员,创建人
	StorageTime datetime not null,--实际到库时间
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go

create table StorageInMaterialsLeader--物资组长审核表
(
	MaterialsLeaderID int identity(1,1) primary key not null, --主键
	MaterialsID int not null foreign key references StorageInMaterials(StorageInMaterialsID),--所属物资组信息表,外键
	Auditing nvarchar(10) not null check(Auditing in ('是','否')),--审核是否通过
	Auditingidea nvarchar(200),--审核意见
	Creator int not null foreign key references EmpInfo(EmpID),--物资组长,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注

)
go

create table StorageInTest--质检信息表
(
	StorageInTestID int identity(1,1) primary key not null, --主键
	MaterialsLeaderID int not null foreign key references StorageInMaterialsLeader(MaterialsLeaderID),--所属物资组长信息表,外键
	TestGentaojian decimal(18,2),--合格根/台/套/件数量
    TestMetre decimal(18,2), --合格米的数量
    TestTon decimal(18,2),--合格吨的数量
	FailedGentaojian decimal(18,2),--质检不合格根/台/套/件数量
	FailedMetre decimal(18,2), --质检不合格米的数量
    FailedTon decimal(18,2),--质检不合格吨的数量
	InspectionReportNum nvarchar(50) not null,--质量检验报告号
	FileNameStr nvarchar(50) not null,--质检报告文档文件名
	Creator int not null foreign key references EmpInfo(EmpID),--质检人员,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go


create table StorageInAssets--资产组信息表
(
	StorageInAssetsID int identity(1,1) primary key not null, --主键
	TestID int not null foreign key references StorageInTest(StorageInTestID),--所属物资组信息表,外键
	BillCode nvarchar(50) not null,--入库单据号
	financeCode nvarchar(50) not null,--财务编号
	CurUnit nvarchar(50) check(CurUnit in ('根/台/套/件','米','吨')),--计量单位
	UnitPrice decimal(18,2) not null,--单价
	Amount decimal(18,2) not null,--金额
	MaterialsAttribute nvarchar(50) not null,--物资属性
	Creator int not null foreign key references EmpInfo(EmpID),--资产组员,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go
create table StorageInHead--资产组长信息表
(
	StorageInHeadID int identity(1,1) primary key not null, --主键
	AssetsID int not null foreign key references StorageInAssets(StorageInAssetsID),--所属资产组信息表,外键
	Auditing nvarchar(10) not null check(Auditing in ('是','否')),--审核是否通过
	Auditingidea nvarchar(200),--审核意见
	Creator int not null foreign key references EmpInfo(EmpID),--资产组长,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go
create table StorageDirector--主任信息表
(
	StorageInDirectorID int identity(1,1) primary key not null, --主键
	HeadID int not null foreign key references StorageInHead(StorageInHeadID),--所属资产组信息表,外键
	Approve nvarchar(10) not null check(Approve in ('是','否')),--审批是否通过
	ApproveIdea nvarchar(200),--审批意见
	Creator int not null foreign key references EmpInfo(EmpID),--主任,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go

create table TaskStorageIn --用户任务列表
(
	TaskStorageID int identity(1,1) primary key not null, --主键
	TaskCreaterID int not null foreign key references EmpInfo(EmpID),--任务发起人 外键
	TaskTargetID int not null foreign key references EmpInfo(EmpID),--任务接受目标 外键
	StorageInType nvarchar(50) check(StorageInType in ('正常入库','委外入库','回收入库')),--入库类型：1-正常入库 2-委外入库 3-回收入库
	StorageInID int not null,--任务关联的入库单ID
	QCBatch nvarchar(50),--质检批次(委外入库无批次)
	TaskTitle nvarchar(50) not null,-- 任务标题
	Remark nvarchar(200), --备注
	InspectState nvarchar(50) check (InspectState in ('未审核','已审核','通过','驳回')),-- 审核状态
	TaskState nvarchar(50) check (TaskState in ('未完成','已完成')),-- 任务状态
	TaskDispose nvarchar(50) default ('未废弃') check(TaskDispose in ('未废弃', '废弃')) ,--任务状态
	TaskType nvarchar(50) check(TaskType in ('生产组','物资组员','物资组长','质检','资产组员','资产组长','主任审核','物资组清点','物资组长确认清点结果','资产组办理回收','回收入库单资产组长确认','维修保养物资组长审核','处理清点问题','处理物资组长审核问题','生产组安排质检','检验员质检','资产组处理合格物资','资产组长确认合格物资','生产组申请维修','检验员检验修复物资','资产组处理修复合格物资')),--任务类型，edit by adonis 2010-10-18 16：20
	PreviousTaskID int,--前序任务,通过此ID可找到清点表中对应的清点信息
	CreateTime datetime not null  --创建时间	
)
go


--create table TableOfStocks   --库存表
--(
	--StocksID int identity(1,1) primary key not null, --主键
	----StorageInID int not null foreign key references StorageIn(StorageInID),--所属入库单,外键
	--StorageInID int not null,--所属入库单
	--StorageInType nvarchar(50) check(StorageInType in ('正常入库','委外入库','回收入库')),--入库类型：1-正常入库 2-委外入库 3-回收入库
	--MaterialID int not null foreign key references MaterialInfo(MaterialID),--物料编码
	--MaterialCode nvarchar(50),--线下空物料编号
	--SpecificationModel nvarchar(50) not null,--规格型号
	--UnitPrice decimal(18,2) not null,--单价
	--NumberQualified decimal(18,2),--合格数量(待删除字段，暂为0)
	--Quantity decimal(18,2) not null,--当前所选单位数量
	--QuantityGentaojian decimal(18,2),--根/台/套/件数量
    --QuantityMetre decimal(18,2), --米的数量
    --QuantityTon decimal(18,2),--吨的数量
    --CurUnit nvarchar(50) check(CurUnit in ('根/台/套/件','米','吨')),--当前计量单位
    --PileID int not null foreign key references PileInfo(PileID),--所属垛位,外键
	--financeCode nvarchar(50) not null,--财务编号
	--StorageTime datetime not null,--到库时间
	--SupplierID int not null foreign key references SupplierInfo(SupplierID),--供应商,外键
	--MaterialsManager int not null foreign key references EmpInfo(EmpID),--物资管理员
	--WarehouseWorker int not null foreign key references EmpInfo(EmpID),--仓库员
	--OnlineState nvarchar(50) default ('线下') check(OnlineState in ('线下', '线上')) ,--物资状态
	----OnlineCode nvarchar(50)
	--Remark nvarchar(200)
--)
--go


create table TableOfStocks   --库存表
(
	StocksID int identity(1,1) primary key not null, --主键
	StorageInID int,--所属入库单(交货通知单编号)
	StorageInType nvarchar(50) check(StorageInType in ('正常入库','委外入库','回收入库')),--入库类型：1-正常入库 2-委外入库 3-回收入库
	
	ReceivingTypeName  nvarchar(50),--正常入库类型
	StorageInCode nvarchar(50),--入库通知单号
	BillCode nvarchar(50),--入库单号(CommitInAssets)
	
	MaterialID int foreign key references MaterialInfo(MaterialID),--物料编码(物料名称,规格型号,财务编码)
	MaterialCode nvarchar(50),--线下空物料编号
	QuantityGentaojian decimal(18,2),--根/台/套/件数量
    QuantityMetre decimal(18,2), --米的数量
    QuantityTon decimal(18,2),--吨的数量
	CurUnit nvarchar(50) check(CurUnit in ('根/台/套/件','米','吨')),--当前计量单位
	UnitPrice decimal(18,2) not null,--单价
	Amount decimal(18,2) not null,--金额
	ExpectedProject int foreign key references ProjectInfo(ProjectID),--预期使用项目
	Remark nvarchar(max), -- 物资属性
	BatchIndex  nvarchar(50),--批次信息
	ManufacturerID int  foreign key references Manufacturer(ManufacturerID),--生产厂家,外键
	SupplierID int foreign key references SupplierInfo(SupplierID),--供应商,外键
	StorageID int foreign key references StorageInfo(StorageID),--所在仓库
	PileID int foreign key references PileInfo(PileID),--所属垛位
	MaterialsManager int foreign key references EmpInfo(EmpID),--物资管理员
	AssetsManager int foreign key references EmpInfo(EmpID),--资产管理员
	StorageTime datetime not null,--实际到库时间
	Creator int foreign key references EmpInfo(EmpID),--创建人
	CreateTime datetime not null  --创建时间
)
go

create table StockOnline   --线上收料表(线上库存)
(
	StockOnlineID  int identity(1,1) primary key not null, --主键
	--TableOfStocksID int not null foreign key references TableOfStocks(StocksID),--所属库存,外键
	
	
	StorageInID int,--所属入库单(交货通知单编号)
	StorageInType nvarchar(50) check(StorageInType in ('正常入库','委外入库','回收入库')),--入库类型：1-正常入库 2-委外入库 3-回收入库
	ReceivingTypeName  nvarchar(50),--正常入库类型
	StorageInCode nvarchar(50),--入库通知单号
	BillCode nvarchar(50),--入库单号(CommitInAssets)
	MaterialID int foreign key references MaterialInfo(MaterialID),--物料编码(物料名称,规格型号,财务编码)
	MaterialCode nvarchar(50),--线下空物料编号
	OfflineGentaojian decimal(18,2),--根/台/套/件数量
    OfflineMetre decimal(18,2), --米的数量
    OfflineTon decimal(18,2),--吨的数量
	CurUnit nvarchar(50) check(CurUnit in ('根/台/套/件','米','吨')),--当前计量单位
	UnitPrice decimal(18,2) not null,--单价
	Amount decimal(18,2) not null,--金额
	ExpectedProject int foreign key references ProjectInfo(ProjectID),--预期使用项目
	Remark nvarchar(max), -- 物资属性
	BatchIndex  nvarchar(50),--批次信息
	ManufacturerID int  foreign key references Manufacturer(ManufacturerID),--生产厂家,外键
	SupplierID int foreign key references SupplierInfo(SupplierID),--供应商,外键
	StorageID int foreign key references StorageInfo(StorageID),--所在仓库
	PileID int foreign key references PileInfo(PileID),--所属垛位
	MaterialsManager int foreign key references EmpInfo(EmpID),--物资管理员
	AssetsManager int foreign key references EmpInfo(EmpID),--资产管理员
	StorageTime datetime not null,--实际到库时间
	
	OrderNum nvarchar(50),--采购订单号
	CertificateNum nvarchar(50),--收料凭证号
	OnlineCode nvarchar(50),--线上物料编号
	OnlineUnit nvarchar(50) check(OnlineUnit in ('根/台/套/件','米','吨')),--线上物资计量单位
	QuantityGentaojian decimal(18,2),--根/台/套/件数量
	QuantityMetre decimal(18,2), --米的数量
    QuantityTon decimal(18,2),--吨的数量
	CurQuantity decimal(18,2),--当前单位数量
	OnlineUnitPrice decimal(18,2), --线上收料单价,金额/数量
	OnlineTotal decimal(18,2), -- 线上收料金额
	OnlineDate datetime, --线上收料时间
	
	Creator int  foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null  --创建时间
)
go


create table  FlowDetailsOffline   --物资流向表(线下库存)
(
	FlowDetailsID  int identity(1,1) primary key not null, --主键
	TableOfStocksID int not null foreign key references TableOfStocks(StocksID),--所属库存,外键
	StorageType nvarchar(50) check(StorageType in ('正常出库','委外出库')),--入库类型：1-正常出库 2-委外出库 
	StorageOutCode nvarchar(50) not null,--调拨单编号
	StorageOutProject int not null foreign key references ProjectInfo(ProjectID),--出库单所属项目
	CurUnit nvarchar(50) check(CurUnit in ('根/台/套/件','米','吨')),--当前计量单位
	RealGentaojian decimal(18,2) not null,--根/台/套/件数量(备注)
    RealMetre decimal(18,2) not null, --米的数量(备注)
    RealTon decimal(18,2) not null,--吨的数量(备注)
    CurQuantity decimal(18,2) not null,--当前单位下物资数量 
    Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null  --创建时间
)




--*********************************************************
--正常出库流程相关数据表
--*********************************************************

go

create table StorageOutTask --出库流程用户任务列表
(
	TaskID int identity(1,1) primary key not null, --主键
	Process nvarchar(50) check(Process in ('正常出库','委外出库')),--出库类型
	TaskCreaterID int not null foreign key references EmpInfo(EmpID),--任务发起人 外键
	TaskTargetID int not null foreign key references EmpInfo(EmpID),--任务接受目标 外键	
	NoticeID int not null,--涉及调拨通知单	
	TaskTitle nvarchar(50) not null,-- 任务标题
	Remark nvarchar(200), --备注
	TaskState nvarchar(50) check (TaskState in ('未完成','已完成')),-- 任务状态
	TaskDispose nvarchar(50) default ('未废弃') check(TaskDispose in ('未废弃', '废弃')) ,--任务状态
	TaskType nvarchar(50) check(TaskType in ('物资调拨审核信息','物资出库审核信息','物资调拨审核','物资出库','物资出库审核','主任审批')),
	CreateTime datetime not null,  --创建时间
	PreviousTaskID int not null--前序任务
)

go

create table StorageOutNotice        --物资设备调拨通知表
(
	StorageOutNoticeID int identity(1,1) primary key not null,--主键
	StorageOutNoticeCode nvarchar(50) not null unique, --调拨通知单编号
	ProjectStage nvarchar(20) not null check(ProjectStage in ('钻井','完井','测试','地面建设','其他')),
	ProjectID int not null foreign key references ProjectInfo(ProjectID),--项目
	Proprietor int not null foreign key references BusinessUnitInfo(BusinessUnitID),--业主单位
	Constructor int not null foreign key references BusinessUnitInfo(BusinessUnitID),--施工单位
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null,--创建日期
	Remark nvarchar(200)
)

go

create table StorageOutDetails    --物资调拨明细表
(
	StorageOutDetailsID int identity(1,1) primary key not null,--主键
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID) on delete cascade,--所属调拨通知单	
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--物料	
	Gentaojian decimal(18,2),--根/台/套/件数量
    Metre decimal(18,2), --米的数量
    Ton decimal(18,2),--吨的数量	
	CreateTime datetime not null,--创建日期
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)	
)

go
create table StorageOutProduceAudit   --出库生产组长审核表
(
	StorageOutProduceAuditID int identity(1,1) primary key not null,--主键
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID),--调拨通知单
	AuditStatus nvarchar(10) not null,--审核状态(通过,未通过)
	AuditOpinion nvarchar(200),--审核意见
	AuditTime datetime not null,--审核时间
	ProduceChief int not null foreign key references EmpInfo(EmpID),--生产组长
	TaskID int not null foreign key references StorageOutTask(TaskID)--来自哪个任务		
)

go
create table StorageOutRealDetails    --出库明细表
(
	StorageOutRealDetailsID int identity(1,1) primary key not null,--主键
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID),--所属调拨通知单
	StorageOutDetailsID int not null foreign key references StorageOutDetails(StorageOutDetailsID),--对应的调拨物资
	StocksID int not null,--调拨的物资ID
	MaterialStatus nvarchar(10) not null check(MaterialStatus in('线上','线下')),
	RealGentaojian decimal(18,2) not null,--根/台/套/件数量
    RealMetre decimal(18,2) not null, --米的数量
    RealTon decimal(18,2) not null,--吨的数量	
	RealAmount decimal(18,2) not null,--实际金额
	CreateTime datetime not null,--创建日期
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)	
)

go
create table StorageOutAssetAudit --出库资产组长审核表
(
	StorageOutAssetAuditID int identity(1,1) primary key not null,--主键
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID),--调拨通知单号
	StorageOutProduceAuditID int not null foreign key references StorageOutProduceAudit(StorageOutProduceAuditID),--关联生产组长审核
	AuditStatus nvarchar(10) not null,--审核状态 
	AuditOpinion nvarchar(200),--审核意见		
	AuditTime datetime not null,--审核时间
	AssetChief int not null foreign key references EmpInfo(EmpID),--资产组长
	TaskID int not null foreign key references StorageOutTask(TaskID)--来自哪个任务
)
go

create table StorageOutDirectorConfirm --出库主任确认
(
	StorageOutDirectorConfirmID int identity(1,1) primary key not null,--主键
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID),--调拨通知单号	
	StorageOutAssetAuditID int not null foreign key references StorageOutAssetAudit(StorageOutAssetAuditID),--资产组长审核ID	
	ConfirmTime datetime not null,--主任审核时间
	Director int not null foreign key references EmpInfo(EmpID),--主任
	TaskID int not null foreign key references StorageOutTask(TaskID)--来自哪个任务		
)
go

--**************************************************************
--主任代理
--**************************************************************
create table TaskProxyType --委托任务类型
(
	TaskProxyTypeID int identity(1,1) primary key not null, --主键
	TaskProxyTypeName nvarchar(50) not null--委托任务类型名称
)
go

create table TaskProxy--主任代理任务表
(
	TaskProxyID  int identity(1,1) primary key not null, --主键
	ProxyPrincipal int not null foreign key references EmpInfo(EmpID),--委托人
	ProxyFiduciary int not null foreign key references EmpInfo(EmpID), --受托人
	ProxyTaskType  int not null foreign key references TaskProxyType(TaskProxyTypeID),-- 委托任务类型
	StartTime datetime not null,--任务开始日期
	EndTime datetime not null,--任务结束日期
	CreateTime datetime not null,--创建日期
	TaskDispose nvarchar(50) default ('待处理') check(TaskDispose in ('待处理','处理中','已过期')) ,--任务状态
	Remark nvarchar(200)--备注
)
go
create table ProxyDirector --主任与任务关系表
(
	ProxyDirectorID int identity(1,1) primary key not null, --主键
	TaskID int not null,--任务ID
	TaskProxyID  int not null foreign key references TaskProxy(TaskProxyID),--代理任务ID
)
go

--*********************************************************
--委外出库流程相关数据表
--Created By: Xu Chun Lei
--Date:2010.07.05-2010.07.06
--*********************************************************

create table StorageCommitOutNotice        --物资设备委外调拨通知表
(
	StorageCommitOutNoticeID int identity(1,1) primary key not null,--主键
	StorageCommitOutNoticeCode nvarchar(50) not null unique, --调拨通知单编号
	Receiver int not null foreign key references BusinessUnitInfo(BusinessUnitID),--领料单位
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null,--创建日期
	Remark nvarchar(200)
)

go

create table StorageCommitOutDetails    --委外物资调拨明细表
(
	StorageCommitOutDetailsID int identity(1,1) primary key not null,--主键
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID) on delete cascade,--所属调拨通知单	
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--物料	
	Gentaojian decimal(18,2),--根/台/套/件数量
    Metre decimal(18,2), --米的数量
    Ton decimal(18,2),--吨的数量	
	CreateTime datetime not null,--创建日期
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)	
)

go
create table StorageCommitOutProduceAudit   --委外出库生产组长审核表
(
	StorageCommitOutProduceAuditID int identity(1,1) primary key not null,--主键
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID),--调拨通知单
	AuditStatus nvarchar(10) not null,--审核状态(通过,未通过)
	AuditOpinion nvarchar(200),--审核意见
	AuditTime datetime not null,--审核时间
	ProduceChief int not null foreign key references EmpInfo(EmpID),--生产组长
	TaskID int not null foreign key references StorageOutTask(TaskID)--来自哪个任务		
)

go
create table StorageCommitOutRealDetails    --委外出库明细表
(
	StorageCommitOutRealDetailsID int identity(1,1) primary key not null,--主键
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID),--所属调拨通知单
	StorageCommitOutDetailsID int not null foreign key references StorageCommitOutDetails(StorageCommitOutDetailsID),--对应的调拨物资
	StocksID int not null,--调拨的物资ID
	MaterialStatus nvarchar(10) not null check(MaterialStatus in('线上','线下')),
	RealGentaojian decimal(18,2) not null,--根/台/套/件数量
    RealMetre decimal(18,2) not null, --米的数量
    RealTon decimal(18,2) not null,--吨的数量	
	RealAmount decimal(18,2) not null,--实际金额
	CreateTime datetime not null,--创建日期
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)	
)

go
create table StorageCommitOutAssetAudit --委外出库资产组长审核表
(
	StorageCommitOutAssetAuditID int identity(1,1) primary key not null,--主键
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID),--调拨通知单号
	StorageCommitOutProduceAuditID int not null foreign key references StorageCommitOutProduceAudit(StorageCommitOutProduceAuditID),--关联生产组长审核
	AuditStatus nvarchar(10) not null,--审核状态 
	AuditOpinion nvarchar(200),--审核意见		
	AuditTime datetime not null,--审核时间
	AssetChief int not null foreign key references EmpInfo(EmpID),--资产组长
	TaskID int not null foreign key references StorageOutTask(TaskID)--来自哪个任务
)
go

create table StorageCommitOutDirectorConfirm --委外出库主任确认
(
	StorageCommitOutDirectorConfirmID int identity(1,1) primary key not null,--主键
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID),--调拨通知单号	
	StorageCommitOutAssetAuditID int not null foreign key references StorageCommitOutAssetAudit(StorageCommitOutAssetAuditID),--资产组长审核ID	
	ConfirmTime datetime not null,--主任审核时间
	Director int not null foreign key references EmpInfo(EmpID),--主任
	TaskID int not null foreign key references StorageOutTask(TaskID)--来自哪个任务		
)
go

--*********************************************************
--委外入库相关数据表(add by adonis)
--*********************************************************




create table CommitInMain --入库主表
(
	StorageInID int identity(1,1) primary key not null, --主键
	StorageInCode nvarchar(50) not null,--交货通知单编号
    ReceivingType nvarchar(50) not null,--入库类型	    
	Remark nvarchar(200),--备注
	StorageInQualifiedNum nvarchar(50),--质检合格入库编号
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null--创建时间
)
go
create table CommitProduce--生产组信息表
(
	StorageInProduceID int identity(1,1) primary key not null, --主键
	StorageInID int not null foreign key references CommitInMain(StorageInID),--所属入库单,外键
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--物料
	QuantityGentaojian decimal(18,2),--根/台/套/件数量
    QuantityMetre decimal(18,2), --米的数量
    QuantityTon decimal(18,2),--吨的数量
    ExpectedProject int not null foreign key references ProjectInfo(ProjectID),--预期使用项目
	ExpectedTime datetime not null,--预期到库时间
	BatchIndex  nvarchar(50),--批次信息
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)--备注
)
go


create table CommitInMaterials--物资组信息表
(
	StorageInMaterialsID int identity(1,1) primary key not null, --主键
	ProduceID int not null foreign key references CommitProduce(StorageInProduceID),--所属生产组信息表,外键
	RealGentaojian decimal(18,2),--根/台/套/件数量
    RealMetre decimal(18,2), --米的数量
    RealTon decimal(18,2),--吨的数量
	ManufacturerID int not null foreign key references Manufacturer(ManufacturerID),--生产厂家ID,外键
	IsManufacturer nvarchar(10) not null check(IsManufacturer in ('是','否')),--生产厂家信息是否与采购合同一致
	SupplierID int not null foreign key references SupplierInfo(SupplierID),--供应商ID,外键
	Supplier nvarchar(10) not null check(Supplier in ('是','否')),--供应商信息是否与采购合同一致
	Data nvarchar(10) not null check(Data in ('是','否')),--资料是否齐全
	Standard nvarchar(10) not null check(Standard in ('是','否')),--制造标准是否与采购合同一致
	Parts nvarchar(10) not null check(Parts in ('是','否')),--配件是否齐全
	Appearance nvarchar(10) not null check(Appearance in ('是','否')),--外观是否完好
	PileID int not null foreign key references PileInfo(PileID),--所属仓库,所属垛位,外键
	Creator int not null foreign key references EmpInfo(EmpID),--物资管理员,创建人
	StorageTime datetime not null,--实际到库时间
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go

create table CommitInMaterialsLeader--物资组长审核表
(
	MaterialsLeaderID int identity(1,1) primary key not null, --主键
	MaterialsID int not null foreign key references CommitInMaterials(StorageInMaterialsID),--所属物资组信息表,外键
	Auditing nvarchar(10) not null check(Auditing in ('是','否')),--审核是否通过
	Auditingidea nvarchar(200),--审核意见
	Creator int not null foreign key references EmpInfo(EmpID),--物资组长,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注

)
go

create table CommitInTest--质检信息表
(
	StorageInTestID int identity(1,1) primary key not null, --主键
	MaterialsLeaderID int not null foreign key references CommitInMaterialsLeader(MaterialsLeaderID),--所属物资组长信息表,外键
	TestGentaojian decimal(18,2),--合格根/台/套/件数量
    TestMetre decimal(18,2), --合格米的数量
    TestTon decimal(18,2),--合格吨的数量
	FailedGentaojian decimal(18,2),--质检不合格根/台/套/件数量
	FailedMetre decimal(18,2), --质检不合格米的数量
    FailedTon decimal(18,2),--质检不合格吨的数量
	InspectionReportNum nvarchar(50) not null,--质量检验报告号
	FileNameStr nvarchar(50) not null,--质检报告文档文件名
	Creator int not null foreign key references EmpInfo(EmpID),--质检人员,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go


create table CommitInAssets--资产组信息表
(
	StorageInAssetsID int identity(1,1) primary key not null, --主键
	TestID int not null foreign key references CommitInTest(StorageInTestID),--所属物资组信息表,外键
	BillCode nvarchar(50) not null,--入库单据号
	financeCode nvarchar(50) not null,--财务编号
	CurUnit nvarchar(50) check(CurUnit in ('根/台/套/件','米','吨')),--计量单位
	UnitPrice decimal(18,2) not null,--单价
	Amount decimal(18,2) not null,--金额
	MaterialsAttribute nvarchar(50) not null,--物资属性
	Creator int not null foreign key references EmpInfo(EmpID),--资产组员,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go
create table CommitInHead--资产组长信息表
(
	StorageInHeadID int identity(1,1) primary key not null, --主键
	AssetsID int not null foreign key references CommitInAssets(StorageInAssetsID),--所属资产组信息表,外键
	Auditing nvarchar(10) not null check(Auditing in ('是','否')),--审核是否通过
	Auditingidea nvarchar(200),--审核意见
	Creator int not null foreign key references EmpInfo(EmpID),--资产组长,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go
create table CommitDirector--主任信息表
(
	StorageInDirectorID int identity(1,1) primary key not null, --主键
	HeadID int not null foreign key references CommitInHead(StorageInHeadID),--所属资产组信息表,外键
	Approve nvarchar(10) not null check(Approve in ('是','否')),--审批是否通过
	ApproveIdea nvarchar(200),--审批意见
	Creator int not null foreign key references EmpInfo(EmpID),--主任,创建人
    CreateTime datetime not null,--创建时间
	Remark nvarchar(200)--备注
)
go


create table RelationCommitIn --委外入库与委外出库关系表
(
	RelationID int identity(1,1) primary key not null, --主键
	CommitMaterial int not null foreign key references CommitProduce(StorageInProduceID),--新建委外物资
    CommitOutMaterial  nvarchar(50) not null,--新建物资来源的委外出库物资(可能是多个,StorageCommitOutRealDetails表id)
	CreateTime datetime not null--创建时间
)
go





--*********************************************************
--移库流程相关数据表
--*********************************************************

create table StockTransfer   -- 移库主表
(
	StockTransferID int identity(1,1) primary key not null,--主键
	StockTransferNum nvarchar(50) not null, --审批编号
	CreateTime  datetime not null ,--申请时间	
	Creater int not null  foreign key references EmpInfo(EmpID), --任务创建人
	Remark nvarchar(200)	
)
go
create table StockTransferTask --审批内容记录表
(
	StockTransferTaskID int identity(1,1) primary key not null,--主键
	StockTransferID int not null ,
	TaskCreaterID int foreign key references EmpInfo(EmpID),--任务发起人 外键
	TaskTargetID int  foreign key references EmpInfo(EmpID),--任务接受目标 外键
	TaskInType  nvarchar(50) check(TaskInType in('移库任务')),--任务类型	
	TaskTitle nvarchar(50) not null, --任务标题
	AcceptTime datetime, --通过時間
	AuditOpinion nvarchar(200),--审核意见
	AuditStatus nvarchar(50) default ('未审核') check(AuditStatus in ('未审核', '审核通过','审核未通过')) ,--审核状态(未审核,审核通过,审核未通过)
	TaskState nvarchar(50) check (TaskState in ('未完成','已完成')),-- 任务状态
	TaskDispose nvarchar(50) default ('未废弃') check(TaskDispose in ('未废弃', '废弃')) ,--任务状态
	TaskType nvarchar(50) check(TaskType in ('物资组长审核信息','发起人修改')),
	CreateTime  datetime not null ,--申请时间	
	Remark nvarchar(200)	
)
go
create table StockTransferDetail --与移库主表关联的清单
(
	StockTransferDetailID  int identity(1,1) primary key not null,--主键
	StockTransferID  int not null,
	DetailType  nvarchar(50) check(DetailType in('移库任务')),--任务类型	
	StocksID int not null,--调拨的物资
	StocksStatus nvarchar(50)check(StocksStatus in('线上','线下','回收合格')),--物资状态,与stocksid 构建成唯一标识
	--Quantity decimal(18,2) not null,--调拨数量
	TargetPile int not null foreign key references PileInfo(PileID), --需要移動到的垛位		
	QuantityGentaojian decimal(18,2),--根/台/套/件数量
    QuantityMetre decimal(18,2), --米的数量
    QuantityTon decimal(18,2),--吨的数量
	Remark nvarchar(200)	
)
go

--*********************************************************
--回收入库流程相关数据表
--Created By: Xu Chun Lei
--Date:2010.07.26-2010.08.19
--*********************************************************
create table SrinSubDoc--回收分单（配送组将一个大单划分为若干分单）
(
	SrinSubDocID int identity(1,1) primary key not null,--主键	
	Project int not null foreign key references ProjectInfo(ProjectID),--所属项目
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null,--创建日期
	Taker int not null foreign key references EmpInfo(EmpID),--承接者	
	Remark nvarchar(200)
)

go

create table SrinSubDetails--回收分单明细-配送组生成
(
	SrinSubDetailsID int identity(1,1) primary key not null,--主键
	SrinSubDocID int not null foreign key references SrinSubDoc(SrinSubDocID) on delete cascade,--回收分单	
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--物料	
	TotleGentaojian decimal(18,2),--根/台/套/件数量
    TotleMetre decimal(18,2), --米的数量
    TotleTon decimal(18,2),--吨的数量   
    RetrieveCode nvarchar(50),--回收单号
	CreateTime datetime not null,--进库日期
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)
)
go

create table SrinStocktaking--物资管理员清点表
(
	SrinStocktakingID int identity(1,1) primary key not null,--主键
	SrinSubDocID int not null foreign key references SrinSubDoc(SrinSubDocID),--清点的回收分单
	StocktakingResult nvarchar(10) not null check(StocktakingResult in('清点有误','清点无误')),--清点结果
	StocktakingDate datetime not null,--清点日期	
	StocktakingProblem nvarchar(max),--清点问题
	Creator int not null foreign key references EmpInfo(EmpID),--清点人
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--来自哪个任务
)
go

create table SrinStocktakingDetails--清点物资明细
(
	SrinStocktakingDetailsID int identity(1,1) primary key not null,--主键
	SrinSubDetailsID int not null foreign key references SrinSubDetails(SrinSubDetailsID),--来自哪个回收分单
	SrinStocktakingID int not null foreign key references SrinStocktaking(SrinStocktakingID),--所属的清点清单
	StorageID int foreign key references StorageInfo(StorageID),--所在仓库
	PileID int foreign key references PileInfo(PileID),--所属垛位
	CreateTime datetime not null,--创建日期
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)
)

go

create table SrinStocktakingConfirm--物资组长确认清点表
(
	SrinStocktakingConfirmID int identity(1,1) primary key not null,--主键
	SrinStocktakingID int not null foreign key references SrinStocktaking(SrinStocktakingID),--确认的清点清单
	MaterialChief int not null foreign key references EmpInfo(EmpID),--物资组长
	ConfirmTime datetime not null,--创建日期
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID),--关联的任务
)

go
create table SrinReceipt--回收物资设备入库单(总单)
(
	SrinReceiptID int identity(1,1) primary key not null, --主键
	SrinStocktakingConfirmID int not null foreign key references SrinStocktakingConfirm(SrinStocktakingConfirmID),--来自的清点后的单据	
	SrinReceiptCode nvarchar(50) not null unique,--回收物资入库单号
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null,--创建日期
	TaskID int foreign key references TaskStorageIn(TaskStorageID),--创建单据的任务
	Remark nvarchar(200)
)
go

create table SrinDetails--回收物资设备入库物资
(
	SrinDetailsID int identity(1,1) primary key not null,--主键
	SrinStocktakingDetailsID int not null foreign key references SrinStocktakingDetails(SrinStocktakingDetailsID),--对应的清点物资
	SrinReceiptID int not null foreign key references SrinReceipt(SrinReceiptID),--所属回收入库单
	CurUnit nvarchar(50) not null check(CurUnit in ('根/台/套/件','米','吨')),--当前计量单位
    UnitPrice decimal(18,2) not null,--单价	
	Amount decimal(18,2) not null,--金额		
	CreateTime datetime not null,--创建日期
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)
	
)	

go

create table SrinAssetReceiptConfirm--回收物资设备入库单(总单)资产组长确认
(
	SrinAssetReceiptConfirmID int identity(1,1) primary key not null,--主键
	SrinReceiptID int not null foreign key references SrinReceipt(SrinReceiptID),--关联的回收入库单
	MaterialChief int not null foreign key references EmpInfo(EmpID),--物资组长
	ConfirmTime datetime not null,--创建日期
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID),--关联的任务
)

go

create table SrinRepairPlan--维修保养计划表
(
	SrinRepairPlanID int identity(1,1) primary key not null,--主键
	SrinReceiptID int not null foreign key references SrinReceipt(SrinReceiptID),--所属回收入库单
	SrinRepairPlanCode nvarchar(50) not null unique,--维修保养计划表编号
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID),--来自哪个任务
)

go
create table SrinMaterialRepairDetails--维修保养表明细--物资管理员
(
	SrinMaterialRepairDetailsID int identity(1,1) primary key not null,--主键
	SrinRepairPlanID int not null foreign key references SrinRepairPlan(SrinRepairPlanID) on delete cascade,--所属维修保养计划表
	SrinDetailsID int not null foreign key references SrinDetails(SrinDetailsID),--回收入库物资
	Gentaojian decimal(18,2) not null,--维修保养数量
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--生产厂家,外键
	ArrivalTime datetime not null,--到库时间
	RepairReason nvarchar(200),--维修原因
	PlanTime datetime,--计划完成时间
	RealTime datetime,--实际完成时间
	RealGentaojian decimal(18,2) not null,--实际维修保养数量
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID)--创建者
	
)
go

create table SrinMaterialRepairAudit--维修保养表--物资组长审核
(
	SrinMaterialRepairAuditID int identity(1,1) primary key not null,--主键
	SrinRepairPlanID int not null foreign key references SrinRepairPlan(SrinRepairPlanID),--所审核的维修保养计划表
	AuditResult nvarchar(10) not null check(AuditResult in('通过','未通过')),--审核状态(通过,未通过)
	AuditOpinion nvarchar(200),--审核意见
	AuditTime datetime not null,--审核时间
	MaterialChief int not null foreign key references EmpInfo(EmpID),--物资组长	
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--来自哪个任务
)

go
create table SrinVerifyTransfer--回收物资检验传递表
(
	SrinVerifyTransferID int identity(1,1) primary key not null,--主键
	SrinReceiptID int not null foreign key references SrinReceipt(SrinReceiptID),--所属回收入库单
	SrinVerifyTransferCode nvarchar(50) not null unique,--回收物资检验传递表编号
	ReadyWorkIsFinished bit not null default(1),--准备工作是否完成
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--来自哪个任务
)
go
create table SrinMaterialVerifyDetails--回收检验物资--物资管理员
(
	SrinMaterialVerifyDetailsID int identity(1,1) primary key not null,--主键
	SrinVerifyTransferID int not null foreign key references SrinVerifyTransfer(SrinVerifyTransferID) on delete cascade,--所属回收物资检验传递表
	SrinDetailsID int not null foreign key references SrinDetails(SrinDetailsID),--回收入库物资	
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--生产厂家,外键
	RetrieveTime datetime not null,--回收时间
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID)--创建者	
)

go

create table SrinProduceVerifyTransfer--回收物资检验传递表-生产组确定需质检时间后
(
	SrinProduceVerifyTransferID int identity(1,1) primary key not null,--主键
	SrinVerifyTransferID int not null foreign key references SrinVerifyTransfer(SrinVerifyTransferID),--关联的回收物资检验传递表
	VerifyTime datetime not null,
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--来自哪个任务
)
go

create table SrinInspectorVerifyTransfer--回收物资检验传递表-质检人员质检之后
(
	SrinInspectorVerifyTransferID int identity(1,1) primary key not null,--主键
	SrinProduceVerifyTransferID int not null foreign key references SrinProduceVerifyTransfer(SrinProduceVerifyTransferID),--关联的回收物资检验传递表	
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--来自哪个任务
)
go

create table SrinInspectorVerifyDetails--回收检验物资-质检结果
(
	SrinInspectorVerifyDetailsID int identity(1,1) primary key not null,--主键
	SrinInspectorVerifyTransferID int not null foreign key references SrinInspectorVerifyTransfer(SrinInspectorVerifyTransferID),--回收检验传递表
	SrinMaterialVerifyDetailsID int not null foreign key references SrinMaterialVerifyDetails(SrinMaterialVerifyDetailsID),--关联的物资管理员回收检验物资
	QualifiedGentaojian decimal(18,2) not null,--合格数量
	RepairGentaojian decimal(18,2) not null,--待维修数量
	RejectGentaojian decimal(18,2) not null,--待报废数量
	VerifyCode nvarchar(50),--质检报告号
	RealVerifyTime datetime not null,--实际质检时间
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID)--创建者
)

go

create table SrinQualifiedReceipt--回收物资设备（合格）入库单
(
	SrinQualifiedReceiptID int identity(1,1) primary key not null, --主键
	SrinInspectorVerifyTransferID int not null foreign key references SrinInspectorVerifyTransfer(SrinInspectorVerifyTransferID),--关联的回收物资检验传递表		
	SrinQualifiedReceiptCode nvarchar(50) not null unique,--回收物资入库单号
	NeedWriteOff bit not null,--是否需要冲销
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null,--创建日期
	TaskID int foreign key references TaskStorageIn(TaskStorageID),--创建单据的任务
	Remark nvarchar(200)
)

go
create table SrinAssetQualifiedDetails--回收入库单（合格）物资--资产管理员
(
	SrinAssetQualifiedDetailsID int identity(1,1) primary key not null,--主键
	SrinQualifiedReceiptID int not null foreign key references SrinQualifiedReceipt(SrinQualifiedReceiptID),--回收检验传递表
	SrinInspectorVerifyDetailsID int not null foreign key references SrinInspectorVerifyDetails(SrinInspectorVerifyDetailsID),--对应的质检物资
	Gentaojian decimal(18,2) not null,--合格数量
	Metre decimal(18,2) not null,--米数量
	Ton decimal(18,2) not null,--吨数量
	Amount decimal(18,2) not null,--金额
	OutUnitPrice decimal(18,2) not null,--出库单价(原)
	InUnitPrice decimal(18,2) not null,--入库单价(新)
	CurUnit nvarchar(50) not null check(CurUnit in ('根/台/套/件','米','吨')),--计量单位
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--生产厂家,外键
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID)--创建者
)
go

create table SrinAChiefQReceiptConfirm--回收入库单（合格）资产组长确认
(
	SrinAChiefQReceiptConfirmID int identity(1,1) primary key not null,--主键
	SrinQualifiedReceiptID int not null foreign key references SrinQualifiedReceipt(SrinQualifiedReceiptID),--关联的回收入库单
	AssetChief int not null foreign key references EmpInfo(EmpID),--资产组长
	ConfirmTime datetime not null,--创建日期
	Remark nvarchar(200),
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--关联的任务
)
go

create table QualifiedStocks--回收合格物资库
(
	StocksID int identity(1,1) primary key not null,--主键
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--物资信息：物资名称、规格型号、财务编码
	StorageTime datetime not null,--到库时间
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--生产厂家
	StorageID int not null foreign key references StorageInfo(StorageID),--所在仓库
	PileID int not null foreign key references PileInfo(PileID),--所属垛位
	Gentaojian decimal(18,2) not null,--合格数量
	Metre decimal(18,2) not null,--米数量
	Ton decimal(18,2) not null,--吨数量
	CurUnit nvarchar(50) not null check(CurUnit in ('根/台/套/件','米','吨')),--计量单位
	UnitPrice decimal(18,2) not null,--单价
	Amount decimal(18,2) not null,--金额
	RetrieveTime datetime not null,--回收时间
	RetrieveProjectID int not null foreign key references ProjectInfo(ProjectID),--回收项目
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID)--创建者
)

create table SrinWriteOffDetails--资产组用于冲销的物资
(
	SrinWriteOffDetailsID int identity(1,1) primary key not null,--主键
	SrinQualifiedReceiptID int not null foreign key references SrinQualifiedReceipt(SrinQualifiedReceiptID),--回收检验传递表
	StorageOutRealDetailsID int not null foreign key references StorageOutRealDetails(StorageOutRealDetailsID),--对应的出库物资
	SrinAssetQualifiedDetailsID int not null foreign key references SrinAssetQualifiedDetails(SrinAssetQualifiedDetailsID),--对应的质检合格物资
	Gentaojian decimal(18,2) not null,--合格数量
	Metre decimal(18,2) not null,--米数量
	Ton decimal(18,2) not null,--吨数量
	Amount decimal(18,2) not null,--金额
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID)--创建者
)

go
create table SrinRepairReport--生产组待修复报告表
(
	SrinRepairReportID int identity(1,1) primary key not null,--主键
	SrinInspectorVerifyTransferID int not null foreign key references SrinInspectorVerifyTransfer(SrinInspectorVerifyTransferID),--关联的回收检验传递表
	SrinRepairReportCode nvarchar(50) not null unique,--报告号	
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--关联的任务
)

go

create table SrinInspectorVerifyRDetails--检验修复物资--质检结果
(
	SrinInspectorVerifyRDetailsID int identity(1,1) primary key not null,--主键
	SrinInspectorVerifyDetailsID int not null foreign key references SrinInspectorVerifyDetails(SrinInspectorVerifyDetailsID),--对应的质检物资
	QualifiedGentaojian decimal(18,2) not null,--合格数量	
	RejectGentaojian decimal(18,2) not null,--待报废数量
	VerifyCode nvarchar(50),--质检报告号
	VerifyTime datetime not null,--质检时间
	Remark nvarchar(200),
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID)--创建者
)

go

--*********************************************************
--报废流程相关数据表
--Created By: adonis
--Date:2010.8.17-2010.8.21
--*********************************************************
create table AwaitScrap --待报废表
(
	AwaitScrapID int identity(1,1) primary key not null, --主键
	ScrapReportNum nvarchar(50) not null default('未填写'),--报废物资报告号(从流程中写入此表时，不写此字段)
	State nvarchar(10) not null check(State in('待报废','已报废')),--报废状态
	
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--物资信息：物资名称、规格型号、财务编码
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--生产厂家,外键
	Gentaojian decimal(18,2) not null,--报废数量
	StorageID int not null foreign key references StorageInfo(StorageID),--所在仓库
	PileID int not null foreign key references PileInfo(PileID),--所属垛位
	ProjectID int foreign key references ProjectInfo(ProjectID),--回收项目ID
	TransferID int, --来自SrinInspectorVerifyTransfer表，回收检验传递表ID
	TransferType nvarchar(10) not null check(TransferType in ('修复检验','正常检验')),--标识是来自修复后的报废还是直接质检后的报废
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	CreateTime datetime not null,--创建时间
	Remark nvarchar(200)
)
go



create table Scrapped   --报废表
(
	ScrappedID int identity(1,1) primary key not null, --主键
	AwaitScrapID int not null foreign key references AwaitScrap(AwaitScrapID),--待报废表外键
	ScrappedNum nvarchar(50) not null,--报废文件号
	StockID int,--所属总库存表ID
	StockType nvarchar(50),--所属总库存类型(线下，线上，回收)
	ScrappedTime datetime not null,--报废时间
	CreateTime datetime not null,--创建时间
	Creator int not null foreign key references EmpInfo(EmpID),--创建者
	Remark nvarchar(200)
)
go


--*********************************************************
--预警表
--*********************************************************

create table WarningList   --报废表
(
	WarningID int identity(1,1) primary key not null, --主键
	MaterialID int not null unique foreign key references MaterialInfo(MaterialID),--物料编码
	QuantityGentaojian decimal(18,2),--根/台/套/件数量
    QuantityMetre decimal(18,2), --米的数量
    QuantityTon decimal(18,2),--吨的数量
)
go

--*********************************************************
--质检报告表
--*********************************************************
create table FileOfQC
(
	FileID int identity(1,1) primary key not null,
	NameOfFile nvarchar(200) not null,
	FileContent varbinary(max),
	FileSize nvarchar(50) not null,
	FileCreateTime datetime not null,
	FileCreateEmp int foreign key references EmpInfo(EmpID),
	Filed1 nvarchar(80),
	Filed2 nvarchar(80),
	Filed3 nvarchar(80),
    Filed4 nvarchar(80)
)
go

--*********************************************************
--视图区
--*********************************************************
        


create view StorageStocks--物资库存视图
as
SELECT     dbo.TableOfStocks.BatchIndex, dbo.MaterialInfo.MaterialName, dbo.MaterialInfo.SpecificationModel, dbo.MaterialInfo.FinanceCode, 
                      dbo.TableOfStocks.MaterialCode, dbo.PileInfo.PileName, dbo.StorageInfo.StorageName, dbo.TableOfStocks.StocksID, dbo.TableOfStocks.UnitPrice, 
                      dbo.TableOfStocks.StorageTime, dbo.TableOfStocks.Remark, '线下' AS Status, dbo.TableOfStocks.MaterialID, dbo.StorageInfo.StorageID, 
                      dbo.TableOfStocks.PileID, dbo.TableOfStocks.QuantityGentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_2
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) AS StocksGenTaojian, dbo.TableOfStocks.QuantityMetre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_5
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) AS StocksMetre, dbo.TableOfStocks.QuantityTon -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) AS StocksTon, dbo.TableOfStocks.CurUnit, 
                      dbo.Manufacturer.ManufacturerName, dbo.Manufacturer.ManufacturerID
FROM         dbo.TableOfStocks LEFT OUTER JOIN
                      dbo.MaterialInfo ON dbo.TableOfStocks.MaterialID = dbo.MaterialInfo.MaterialID LEFT OUTER JOIN
                      dbo.StorageInfo LEFT OUTER JOIN
                      dbo.PileInfo ON dbo.StorageInfo.StorageID = dbo.PileInfo.StorageID ON dbo.TableOfStocks.PileID = dbo.PileInfo.PileID LEFT OUTER JOIN
                      dbo.SupplierInfo ON dbo.TableOfStocks.SupplierID = dbo.SupplierInfo.SupplierID LEFT OUTER JOIN
                      dbo.Manufacturer ON dbo.TableOfStocks.ManufacturerID = dbo.Manufacturer.ManufacturerID
UNION ALL
SELECT     dbo.StockOnline.BatchIndex, MaterialInfo_1.MaterialName, MaterialInfo_1.SpecificationModel, MaterialInfo_1.FinanceCode, 
                      dbo.StockOnline.OnlineCode, PileInfo_1.PileName, StorageInfo_1.StorageName, dbo.StockOnline.StockOnlineID, dbo.StockOnline.OnlineUnitPrice, 
                      dbo.StockOnline.StorageTime, dbo.StockOnline.Remark, '线上' AS Status, dbo.StockOnline.MaterialID, StorageInfo_1.StorageID, dbo.StockOnline.PileID, 
                      dbo.StockOnline.QuantityGentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线上')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_2
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线上')) AS StocksGenTaojian, dbo.StockOnline.QuantityMetre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_5
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线上')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线上')) AS StocksMetre, dbo.StockOnline.QuantityTon -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线上')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线上')) AS StocksTon, dbo.StockOnline.CurUnit, 
                      Manufacturer_1.ManufacturerName, Manufacturer_1.ManufacturerID
FROM         dbo.StockOnline LEFT OUTER JOIN
                      dbo.MaterialInfo AS MaterialInfo_1 ON dbo.StockOnline.MaterialID = MaterialInfo_1.MaterialID LEFT OUTER JOIN
                      dbo.StorageInfo AS StorageInfo_1 LEFT OUTER JOIN
                      dbo.PileInfo AS PileInfo_1 ON StorageInfo_1.StorageID = PileInfo_1.StorageID ON dbo.StockOnline.PileID = PileInfo_1.PileID LEFT OUTER JOIN
                      dbo.SupplierInfo AS SupplierInfo_1 ON dbo.StockOnline.SupplierID = SupplierInfo_1.SupplierID LEFT OUTER JOIN
                      dbo.Manufacturer AS Manufacturer_1 ON dbo.StockOnline.ManufacturerID = Manufacturer_1.ManufacturerID
UNION ALL
SELECT     'N/A' AS Expr1, MaterialInfo_2.MaterialName, MaterialInfo_2.SpecificationModel, MaterialInfo_2.FinanceCode, 'N/A' AS MaterialCode, 
                      PileInfo_2.PileName, StorageInfo_2.StorageName, dbo.QualifiedStocks.StocksID, dbo.QualifiedStocks.UnitPrice, dbo.QualifiedStocks.StorageTime, 
                      dbo.QualifiedStocks.Remark, '回收合格' AS Status, dbo.QualifiedStocks.MaterialID, dbo.QualifiedStocks.StorageID, dbo.QualifiedStocks.PileID, 
                      dbo.QualifiedStocks.Gentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_3
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) AS StocksGenTaojian, dbo.QualifiedStocks.Metre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_2
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_3
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) AS StocksMetre, dbo.QualifiedStocks.Ton -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) AS StocksTon, dbo.QualifiedStocks.CurUnit, 
                      Manufacturer_2.ManufacturerName, dbo.QualifiedStocks.ManufactureID
FROM         dbo.QualifiedStocks INNER JOIN
                      dbo.Manufacturer AS Manufacturer_2 ON dbo.QualifiedStocks.ManufactureID = Manufacturer_2.ManufacturerID INNER JOIN
                      dbo.ProjectInfo AS ProjectInfo_1 ON dbo.QualifiedStocks.RetrieveProjectID = ProjectInfo_1.ProjectID INNER JOIN
                      dbo.MaterialInfo AS MaterialInfo_2 ON dbo.QualifiedStocks.MaterialID = MaterialInfo_2.MaterialID INNER JOIN
                      dbo.StorageInfo AS StorageInfo_2 ON dbo.QualifiedStocks.StorageID = StorageInfo_2.StorageID INNER JOIN
                      dbo.PileInfo AS PileInfo_2 ON dbo.QualifiedStocks.StorageID = PileInfo_2.PileID
go         
create view WriteOffDetails--冲销视图
   as
   SELECT     dbo.StorageOutRealDetails.StorageOutDetailsID, dbo.StorageOutRealDetails.StorageOutRealDetailsID, 
                      dbo.StorageOutRealDetails.StorageOutNoticeID, dbo.StorageOutRealDetails.StocksID, dbo.StorageOutRealDetails.RealGentaojian -
                          (SELECT     ISNULL(SUM(Gentaojian), 0) AS Expr1
                            FROM          dbo.SrinWriteOffDetails
                            WHERE      (dbo.StorageOutRealDetails.StorageOutRealDetailsID = StorageOutRealDetailsID)) AS RealGentaojian, 
                      dbo.StorageOutRealDetails.RealMetre -
                          (SELECT     ISNULL(SUM(Metre), 0) AS Expr1
                            FROM          dbo.SrinWriteOffDetails AS SrinWriteOffDetails_3
                            WHERE      (dbo.StorageOutRealDetails.StorageOutRealDetailsID = StorageOutRealDetailsID)) AS RealMetre, 
                      dbo.StorageOutRealDetails.RealTon -
                          (SELECT     ISNULL(SUM(Ton), 0) AS Expr1
                            FROM          dbo.SrinWriteOffDetails AS SrinWriteOffDetails_2
                            WHERE      (dbo.StorageOutRealDetails.StorageOutRealDetailsID = StorageOutRealDetailsID)) AS RealTon, 
                      dbo.StorageOutRealDetails.RealAmount -
                          (SELECT     ISNULL(SUM(Amount), 0) AS Expr1
                            FROM          dbo.SrinWriteOffDetails AS SrinWriteOffDetails_1
                            WHERE      (dbo.StorageOutRealDetails.StorageOutRealDetailsID = StorageOutRealDetailsID)) AS RealAmount, dbo.StorageOutNotice.ProjectID, 
                      dbo.StorageStocks.UnitPrice, dbo.StorageStocks.CurUnit, dbo.StorageOutRealDetails.CreateTime, dbo.StorageOutNotice.StorageOutNoticeCode, 
                      dbo.StorageStocks.MaterialName, dbo.StorageStocks.SpecificationModel, dbo.StorageStocks.ManufacturerName, dbo.StorageStocks.MaterialID, 
                      dbo.StorageStocks.FinanceCode, dbo.StorageStocks.Status, dbo.StorageStocks.MaterialCode
FROM         dbo.StorageOutRealDetails INNER JOIN
                      dbo.StorageOutNotice ON dbo.StorageOutRealDetails.StorageOutNoticeID = dbo.StorageOutNotice.StorageOutNoticeID INNER JOIN
                      dbo.StorageStocks ON dbo.StorageOutRealDetails.StocksID = dbo.StorageStocks.StocksID AND 
                      dbo.StorageOutRealDetails.MaterialStatus = dbo.StorageStocks.Status
  go 
   
 create view     NormalIn    ----线下正常入库,移入入库查询表
 as
           
       SELECT     dbo.StorageInMain.StorageInCode, dbo.MaterialInfo.MaterialName, dbo.MaterialInfo.FinanceCode, dbo.StorageInMaterials.StorageTime, 
                      dbo.SupplierInfo.SupplierName, dbo.Manufacturer.ManufacturerName, dbo.StorageInAssets.BillCode, dbo.StorageInAssets.CurUnit, 
                      dbo.StorageInAssets.UnitPrice, dbo.StorageInAssets.Amount, dbo.StorageInTest.TestGentaojian, dbo.StorageInTest.TestMetre, 
                      dbo.StorageInTest.TestTon, dbo.StorageDirector.Approve, dbo.ReceivingTypeInfo.ReceivingTypeName, dbo.ReceivingTypeInfo.ReceivingTypeID, 
                      dbo.MaterialInfo.SpecificationModel
FROM         dbo.StorageDirector INNER JOIN
                      dbo.StorageInHead ON dbo.StorageDirector.HeadID = dbo.StorageInHead.StorageInHeadID INNER JOIN
                      dbo.StorageInAssets ON dbo.StorageInHead.AssetsID = dbo.StorageInAssets.StorageInAssetsID INNER JOIN
                      dbo.StorageInTest ON dbo.StorageInAssets.TestID = dbo.StorageInTest.StorageInTestID INNER JOIN
                      dbo.StorageInMaterialsLeader ON dbo.StorageInTest.MaterialsLeaderID = dbo.StorageInMaterialsLeader.MaterialsLeaderID INNER JOIN
                      dbo.StorageInMaterials ON dbo.StorageInMaterialsLeader.MaterialsID = dbo.StorageInMaterials.StorageInMaterialsID INNER JOIN
                      dbo.StorageProduce ON dbo.StorageInMaterials.ProduceID = dbo.StorageProduce.StorageInProduceID INNER JOIN
                      dbo.StorageInMain ON dbo.StorageProduce.StorageInID = dbo.StorageInMain.StorageInID INNER JOIN
                      dbo.MaterialInfo ON dbo.StorageProduce.MaterialID = dbo.MaterialInfo.MaterialID INNER JOIN
                      dbo.Manufacturer ON dbo.StorageInMaterials.ManufacturerID = dbo.Manufacturer.ManufacturerID INNER JOIN
                      dbo.SupplierInfo ON dbo.StorageInMaterials.SupplierID = dbo.SupplierInfo.SupplierID INNER JOIN
                      dbo.ReceivingTypeInfo ON dbo.StorageInMain.ReceivingType = dbo.ReceivingTypeInfo.ReceivingTypeID
WHERE     (dbo.StorageDirector.Approve = N'是')
go


create view WaitForTest--待检
as 
SELECT     dbo.StorageInMain.StorageInCode, dbo.MaterialInfo.MaterialName, dbo.MaterialInfo.FinanceCode, dbo.MaterialInfo.SpecificationModel, 
                      dbo.StorageInMaterials.StorageTime, dbo.SupplierInfo.SupplierName, dbo.Manufacturer.ManufacturerName, dbo.StorageProduce.QuantityGentaojian, 
                      dbo.StorageProduce.QuantityMetre, dbo.StorageProduce.QuantityTon, dbo.StorageInMaterials.StorageInMaterialsID, dbo.StorageInMaterials.Remark, 
                      dbo.StorageInMaterialsLeader.MaterialsLeaderID, dbo.StorageInMaterialsLeader.Auditing
FROM         dbo.StorageInMaterials INNER JOIN
                      dbo.StorageProduce ON dbo.StorageInMaterials.ProduceID = dbo.StorageProduce.StorageInProduceID INNER JOIN
                      dbo.StorageInMain ON dbo.StorageProduce.StorageInID = dbo.StorageInMain.StorageInID INNER JOIN
                      dbo.MaterialInfo ON dbo.StorageProduce.MaterialID = dbo.MaterialInfo.MaterialID INNER JOIN
                      dbo.SupplierInfo ON dbo.StorageInMaterials.SupplierID = dbo.SupplierInfo.SupplierID INNER JOIN
                      dbo.Manufacturer ON dbo.StorageInMaterials.ManufacturerID = dbo.Manufacturer.ManufacturerID INNER JOIN
                      dbo.StorageInMaterialsLeader ON dbo.StorageInMaterials.StorageInMaterialsID = dbo.StorageInMaterialsLeader.MaterialsID
WHERE     (NOT (dbo.StorageInMaterialsLeader.MaterialsLeaderID IN
                          (SELECT     MaterialsLeaderID
                            FROM          dbo.StorageInTest))) AND (dbo.StorageInMaterialsLeader.Auditing = N'是')
go

create view NormalOut--出库
as 

SELECT     dbo.MaterialInfo.MaterialName, dbo.MaterialInfo.FinanceCode, dbo.MaterialInfo.SpecificationModel, dbo.StorageOutRealDetails.MaterialStatus, 
                      dbo.StorageOutNotice.StorageOutNoticeCode, dbo.StorageOutRealDetails.RealGentaojian, dbo.StorageOutRealDetails.RealMetre, 
                      dbo.StorageOutRealDetails.RealTon, dbo.StorageOutRealDetails.RealAmount, dbo.ProjectInfo.ProjectName, dbo.ProjectInfo.ProjectProperty, 
                      dbo.StorageOutNotice.ProjectStage, dbo.StorageOutRealDetails.Remark, dbo.BusinessUnitInfo.BusinessUnitName, 
                      dbo.StorageOutDirectorConfirm.ConfirmTime, BusinessUnitInfo_1.BusinessUnitName AS own, dbo.StorageStocks.StocksID, 
                      dbo.StorageStocks.ManufacturerName, dbo.StorageStocks.CurUnit, dbo.StorageStocks.UnitPrice, dbo.StorageStocks.MaterialCode
FROM         dbo.StorageOutDetails INNER JOIN
                      dbo.StorageOutNotice ON dbo.StorageOutDetails.StorageOutNoticeID = dbo.StorageOutNotice.StorageOutNoticeID INNER JOIN
                      dbo.StorageOutRealDetails ON dbo.StorageOutDetails.StorageOutDetailsID = dbo.StorageOutRealDetails.StorageOutDetailsID AND 
                      dbo.StorageOutNotice.StorageOutNoticeID = dbo.StorageOutRealDetails.StorageOutNoticeID INNER JOIN
                      dbo.StorageOutProduceAudit ON dbo.StorageOutNotice.StorageOutNoticeID = dbo.StorageOutProduceAudit.StorageOutNoticeID INNER JOIN
                      dbo.StorageOutAssetAudit ON dbo.StorageOutNotice.StorageOutNoticeID = dbo.StorageOutAssetAudit.StorageOutNoticeID AND 
                      dbo.StorageOutProduceAudit.StorageOutProduceAuditID = dbo.StorageOutAssetAudit.StorageOutProduceAuditID INNER JOIN
                      dbo.StorageOutDirectorConfirm ON dbo.StorageOutNotice.StorageOutNoticeID = dbo.StorageOutDirectorConfirm.StorageOutNoticeID AND 
                      dbo.StorageOutAssetAudit.StorageOutAssetAuditID = dbo.StorageOutDirectorConfirm.StorageOutAssetAuditID INNER JOIN
                      dbo.MaterialInfo ON dbo.StorageOutDetails.MaterialID = dbo.MaterialInfo.MaterialID INNER JOIN
                      dbo.ProjectInfo ON dbo.StorageOutNotice.ProjectID = dbo.ProjectInfo.ProjectID INNER JOIN
                      dbo.BusinessUnitInfo ON dbo.StorageOutNotice.Constructor = dbo.BusinessUnitInfo.BusinessUnitID INNER JOIN
                      dbo.BusinessUnitInfo AS BusinessUnitInfo_1 ON dbo.ProjectInfo.Owner = BusinessUnitInfo_1.BusinessUnitID INNER JOIN
                      dbo.StorageStocks ON dbo.StorageOutRealDetails.StocksID = dbo.StorageStocks.StocksID AND 
                      dbo.StorageOutRealDetails.MaterialStatus = dbo.StorageStocks.Status
go                
                      
create view ViewCommitIn--委外入
as                      
SELECT     dbo.CommitInMain.StorageInCode, dbo.MaterialInfo.MaterialName, dbo.MaterialInfo.FinanceCode, dbo.CommitInMaterials.StorageTime, 
                      dbo.SupplierInfo.SupplierName, dbo.Manufacturer.ManufacturerName, dbo.CommitInAssets.BillCode, dbo.CommitInAssets.CurUnit, 
                      dbo.CommitInAssets.UnitPrice, dbo.CommitInAssets.Amount, dbo.CommitInTest.TestGentaojian, dbo.CommitInTest.TestMetre, 
                      dbo.CommitInTest.TestTon, dbo.CommitDirector.Approve, dbo.ReceivingTypeInfo.ReceivingTypeName, dbo.ReceivingTypeInfo.ReceivingTypeID, 
                      dbo.MaterialInfo.SpecificationModel
FROM         dbo.CommitDirector INNER JOIN
                      dbo.CommitInHead ON dbo.CommitDirector.HeadID = dbo.CommitInHead.StorageInHeadID INNER JOIN
                      dbo.CommitInAssets ON dbo.CommitInHead.AssetsID = dbo.CommitInAssets.StorageInAssetsID INNER JOIN
                      dbo.CommitInTest ON dbo.CommitInAssets.TestID = dbo.CommitInTest.StorageInTestID INNER JOIN
                      dbo.CommitInMaterialsLeader ON dbo.CommitInTest.MaterialsLeaderID = dbo.CommitInMaterialsLeader.MaterialsLeaderID INNER JOIN
                      dbo.CommitInMaterials ON dbo.CommitInMaterialsLeader.MaterialsID = dbo.CommitInMaterials.StorageInMaterialsID INNER JOIN
                      dbo.CommitProduce ON dbo.CommitInMaterials.ProduceID = dbo.CommitProduce.StorageInProduceID INNER JOIN
                      dbo.CommitInMain ON dbo.CommitProduce.StorageInID = dbo.CommitInMain.StorageInID INNER JOIN
                      dbo.MaterialInfo ON dbo.CommitProduce.MaterialID = dbo.MaterialInfo.MaterialID INNER JOIN
                      dbo.Manufacturer ON dbo.CommitInMaterials.ManufacturerID = dbo.Manufacturer.ManufacturerID INNER JOIN
                      dbo.SupplierInfo ON dbo.CommitInMaterials.SupplierID = dbo.SupplierInfo.SupplierID INNER JOIN
                      dbo.ReceivingTypeInfo ON dbo.CommitInMain.ReceivingType = dbo.ReceivingTypeInfo.ReceivingTypeID
WHERE     (dbo.CommitDirector.Approve = N'是')
                      
go
create view ReportStocks--报表用视图
as              
                      
     SELECT     dbo.TableOfStocks.BatchIndex, dbo.MaterialInfo.MaterialName, dbo.MaterialInfo.SpecificationModel, dbo.MaterialInfo.FinanceCode, 
                      dbo.TableOfStocks.BillCode, dbo.SupplierInfo.SupplierName, dbo.PileInfo.PileCode, dbo.TableOfStocks.StorageInCode, 
                      dbo.TableOfStocks.MaterialCode, dbo.PileInfo.PileName, dbo.StorageInfo.StorageName, dbo.TableOfStocks.StocksID, dbo.TableOfStocks.UnitPrice, 
                      dbo.TableOfStocks.StorageTime, dbo.TableOfStocks.Remark, '线下' AS Status, dbo.TableOfStocks.MaterialID, dbo.StorageInfo.StorageID, 
                      dbo.TableOfStocks.PileID, dbo.TableOfStocks.QuantityGentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_2
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) AS StocksGenTaojian, dbo.TableOfStocks.QuantityMetre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_5
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) AS StocksMetre, dbo.TableOfStocks.QuantityTon -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '线下')) AS StocksTon, dbo.TableOfStocks.CurUnit, 
                      dbo.Manufacturer.ManufacturerName, dbo.Manufacturer.ManufacturerID
FROM         dbo.TableOfStocks LEFT OUTER JOIN
                      dbo.MaterialInfo ON dbo.TableOfStocks.MaterialID = dbo.MaterialInfo.MaterialID LEFT OUTER JOIN
                      dbo.StorageInfo LEFT OUTER JOIN
                      dbo.PileInfo ON dbo.StorageInfo.StorageID = dbo.PileInfo.StorageID ON dbo.TableOfStocks.PileID = dbo.PileInfo.PileID LEFT OUTER JOIN
                      dbo.SupplierInfo ON dbo.TableOfStocks.SupplierID = dbo.SupplierInfo.SupplierID LEFT OUTER JOIN
                      dbo.Manufacturer ON dbo.TableOfStocks.ManufacturerID = dbo.Manufacturer.ManufacturerID
UNION ALL
SELECT     dbo.StockOnline.BatchIndex, MaterialInfo_1.MaterialName, MaterialInfo_1.SpecificationModel, MaterialInfo_1.FinanceCode, 
                      dbo.StockOnline.StorageInCode, dbo.StockOnline.BillCode, SupplierInfo_1.SupplierName, PileInfo_1.PileCode, dbo.StockOnline.MaterialCode, 
                      PileInfo_1.PileName, StorageInfo_1.StorageName, dbo.StockOnline.StockOnlineID, dbo.StockOnline.UnitPrice, dbo.StockOnline.StorageTime, 
                      dbo.StockOnline.Remark, '线下' AS Status, dbo.StockOnline.MaterialID, StorageInfo_1.StorageID, dbo.StockOnline.PileID, 
                      dbo.StockOnline.QuantityGentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_2
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线下')) AS StocksGenTaojian, dbo.StockOnline.QuantityMetre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_5
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线下')) AS StocksMetre, dbo.StockOnline.QuantityTon -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线下')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '线下')) AS StocksTon, dbo.StockOnline.CurUnit, 
                      Manufacturer_1.ManufacturerName, Manufacturer_1.ManufacturerID
FROM         dbo.StockOnline LEFT OUTER JOIN
                      dbo.MaterialInfo AS MaterialInfo_1 ON dbo.StockOnline.MaterialID = MaterialInfo_1.MaterialID LEFT OUTER JOIN
                      dbo.ProjectInfo ON dbo.StockOnline.ExpectedProject = dbo.ProjectInfo.ProjectID LEFT OUTER JOIN
                      dbo.Manufacturer AS Manufacturer_1 ON dbo.StockOnline.ManufacturerID = Manufacturer_1.ManufacturerID LEFT OUTER JOIN
                      dbo.SupplierInfo AS SupplierInfo_1 ON dbo.StockOnline.SupplierID = SupplierInfo_1.SupplierID LEFT OUTER JOIN
                      dbo.StorageInfo AS StorageInfo_1 ON dbo.StockOnline.StorageID = StorageInfo_1.StorageID LEFT OUTER JOIN
                      dbo.PileInfo AS PileInfo_1 ON dbo.StockOnline.PileID = PileInfo_1.PileID AND StorageInfo_1.StorageID = PileInfo_1.StorageID LEFT OUTER JOIN
                      dbo.EmpInfo ON dbo.StockOnline.MaterialsManager = dbo.EmpInfo.EmpID AND dbo.StockOnline.AssetsManager = dbo.EmpInfo.EmpID AND 
                      dbo.StockOnline.Creator = dbo.EmpInfo.EmpID AND StorageInfo_1.EmpID = dbo.EmpInfo.EmpID
UNION ALL
SELECT     'N/A' AS Expr1, MaterialInfo_2.MaterialName, MaterialInfo_2.SpecificationModel, MaterialInfo_2.FinanceCode, 'N/A' AS MaterialCode, NULL 
                      AS StorageInCode, NULL AS BillCode, NULL AS SupplierName, NULL AS PileCode, PileInfo_2.PileName, StorageInfo_2.StorageName, 
                      dbo.QualifiedStocks.StocksID, dbo.QualifiedStocks.UnitPrice, dbo.QualifiedStocks.StorageTime, dbo.QualifiedStocks.Remark, '回收合格' AS Status, 
                      dbo.QualifiedStocks.MaterialID, dbo.QualifiedStocks.StorageID, dbo.QualifiedStocks.PileID, dbo.QualifiedStocks.Gentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_3
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) AS StocksGenTaojian, dbo.QualifiedStocks.Metre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_2
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_3
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) AS StocksMetre, dbo.QualifiedStocks.Ton -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '回收合格')) AS StocksTon, dbo.QualifiedStocks.CurUnit, 
                      Manufacturer_2.ManufacturerName, dbo.QualifiedStocks.ManufactureID
FROM         dbo.QualifiedStocks INNER JOIN
                      dbo.Manufacturer AS Manufacturer_2 ON dbo.QualifiedStocks.ManufactureID = Manufacturer_2.ManufacturerID INNER JOIN
                      dbo.ProjectInfo AS ProjectInfo_1 ON dbo.QualifiedStocks.RetrieveProjectID = ProjectInfo_1.ProjectID INNER JOIN
                      dbo.MaterialInfo AS MaterialInfo_2 ON dbo.QualifiedStocks.MaterialID = MaterialInfo_2.MaterialID INNER JOIN
                      dbo.StorageInfo AS StorageInfo_2 ON dbo.QualifiedStocks.StorageID = StorageInfo_2.StorageID INNER JOIN
                      dbo.PileInfo AS PileInfo_2 ON dbo.QualifiedStocks.StorageID = PileInfo_2.PileID
go                 
create view FlowDetails--转线上物资流向图
as 

SELECT     dbo.StorageCommitOutNotice.StorageCommitOutNoticeCode, dbo.ProjectInfo.ProjectName, dbo.TableOfStocks.CurUnit, 
                      dbo.StorageCommitOutRealDetails.RealGentaojian, dbo.StorageCommitOutRealDetails.RealMetre, dbo.StorageCommitOutRealDetails.RealTon, 
                      '委外出库' AS type, dbo.TableOfStocks.StocksID, dbo.ProjectInfo.ProjectID
FROM         dbo.ProjectInfo INNER JOIN
                      dbo.TableOfStocks ON dbo.ProjectInfo.ProjectID = dbo.TableOfStocks.ExpectedProject INNER JOIN
                      dbo.StorageCommitOutRealDetails ON dbo.TableOfStocks.StocksID = dbo.StorageCommitOutRealDetails.StocksID INNER JOIN
                      dbo.StorageCommitOutNotice ON 
                      dbo.StorageCommitOutRealDetails.StorageCommitOutNoticeID = dbo.StorageCommitOutNotice.StorageCommitOutNoticeID
UNION ALL
SELECT     dbo.StorageOutNotice.StorageOutNoticeCode, ProjectInfo_1.ProjectName, TableOfStocks_1.CurUnit, dbo.StorageOutRealDetails.RealGentaojian, 
                      dbo.StorageOutRealDetails.RealMetre, dbo.StorageOutRealDetails.RealTon, '正常出库' AS type, TableOfStocks_1.StocksID, 
                      ProjectInfo_1.ProjectID
FROM         dbo.TableOfStocks AS TableOfStocks_1 INNER JOIN
                      dbo.StorageOutRealDetails ON TableOfStocks_1.StocksID = dbo.StorageOutRealDetails.StocksID INNER JOIN
                      dbo.ProjectInfo AS ProjectInfo_1 ON TableOfStocks_1.ExpectedProject = ProjectInfo_1.ProjectID INNER JOIN
                      dbo.StorageOutNotice ON dbo.StorageOutRealDetails.StorageOutNoticeID = dbo.StorageOutNotice.StorageOutNoticeID
                      
                      
                      
                     
                     