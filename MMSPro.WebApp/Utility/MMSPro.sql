use MMSPro
go

--****************************************************************************************
--������Ϣ��
--****************************************************************************************
create table DepInfo
(
	DepID int identity(1,1) primary key not null, --����
	DepName nvarchar(50) not null ,
	DepCode nvarchar(50) not null unique, --ΨһԼ��
	InCharge nvarchar(50),
	Contact nvarchar(50),
	Remark nvarchar(200)
)
go
create table EmpInfo
(
	EmpID int identity(1,1) primary key not null, --����
	Account nvarchar(50) not null unique, --ΨһԼ��
	--PassWord nvarchar(50),
	DepID int not null foreign key references DepInfo(DepID), --���
	EmpName nvarchar(50),
	Contact nvarchar(50),
	Remark nvarchar(200)
)
go
create table SupplierType
(
	SupplierTypeID int identity(1,1) primary key not null, --����
	SupplierTypeName nvarchar(50) not null,
	SupplierTypeCode nvarchar(50) not null,
	Remark nvarchar(200)
)
go
create table SupplierInfo
(
	SupplierID int identity(1,1) primary key not null, --����
	SupplierName nvarchar(50) not null,
	SupplierCode nvarchar(50) not null unique, --ΨһԼ��
	SupplierTypeID int not null foreign key references SupplierType(SupplierTypeID),--���
	SupplierAddress1 nvarchar(200),
	SupplierAddress2 nvarchar(200),
	SupplierPhone nvarchar(50),
	InCharge nvarchar(50),
	Remark nvarchar(200)
)
go

--����δ���ڻ�����Ϣ���轫λ���ᵽǰ��
create table ManufacturerType--��������
(
	ManufacturerTypeID int identity(1,1) primary key not null, --����
	ManufacturerTypeName nvarchar(50) not null,--����������������
	ManufacturerTypeCode nvarchar(50) not null,--�����������ͱ���
	Remark nvarchar(200)
)
go
create table Manufacturer--��������
(
	ManufacturerID int identity(1,1) primary key not null, --����
	ManufacturerName nvarchar(50) not null,--��������
	ManufacturerCode nvarchar(50) not null unique, --�������̱��룬ΨһԼ��
	ManufacturerTypeID int not null foreign key references ManufacturerType(ManufacturerTypeID),--�������ͣ����
	ManufacturerAddress1 nvarchar(200),--�������ҵ�ַ1
	ManufacturerAddress2 nvarchar(200),--�������ҵ�ַ2
	ManufacturerPhone nvarchar(50),--����������ϵ�绰
	principal nvarchar(50),--������
	Remark nvarchar(200)
)
go

create table MaterialType
(
	MaterialTypeID int identity(1,1) primary key not null, --����
	MaterialTypeName nvarchar(50) not null,
	MaterialTypeCode nvarchar(50) not null unique --ΨһԼ��
)
go
create table MaterialMainType
(
	MaterialMainTypeID int identity(1,1) primary key not null, --����
	MaterialMainTypeName nvarchar(50) not null,
	MaterialMainTypeCode nvarchar(50) not null unique ,--ΨһԼ��
	MaterialTypeID int not null foreign key references MaterialType(MaterialTypeID) --���
)
go
create table MaterialChildType
(
	MaterialChildTypeID int identity(1,1) primary key not null, --����
	MaterialChildTypeName nvarchar(50) not null,
	MaterialChildTypeCode nvarchar(50) not null unique, --ΨһԼ��
	MaterialMainTypeID int not null foreign key references MaterialMainType(MaterialMainTypeID) --���
)
go
create table MaterialInfo
(
	MaterialID int identity(1,1) primary key not null, --����
	--MaterialCode  nvarchar(50) not null unique,--ΨһԼ��
	FinanceCode  nvarchar(50),-- ���ϱ���Ϊ"N/A" --�˴��޸�Ϊ������� modify by roro
	MaterialName nvarchar(50) not null,
	MaterialchildTypeID int not null foreign key references MaterialChildType(MaterialChildTypeID), --���
	SpecificationModel  nvarchar(200),--����ͺ� --�˴��޸�Ϊ����ͺ� modify by roro
	Remark nvarchar(200)
)


go
create table BusinessUnitType
(
	BusinessUnitTypeID int identity(1,1) primary key not null, --����
	BusinessUnitTypeName nvarchar(50) not null,
	BusinessUnitTypeCode nvarchar(50) not null unique,--ΨһԼ��
	Remark nvarchar(200)
)
go
create table BusinessUnitInfo
(
	BusinessUnitID int identity(1,1) primary key not null, --����
	BusinessUnitName nvarchar(50) not null,
	BusinessUnitCode nvarchar(50) not null unique, --ΨһԼ��
	BusinessUnitTypeID int not null foreign key references BusinessUnitType(BusinessUnitTypeID), --���
	BusinessUnitAddress1 nvarchar(200),
	BusinessUnitAddress2 nvarchar(200),
	BusinessUnitPhone nvarchar(50),
	InCharger nvarchar(50),
	Remark nvarchar(200)
)

go
create table StorageInfo
(
	StorageID int identity(1,1) primary key not null, --����
	StorageName nvarchar(50) not null,
	StorageCode nvarchar(50) not null unique, --ΨһԼ��
	EmpID int not null foreign key references EmpInfo(EmpID), --���
	Remark nvarchar(200)
)
go
create table PileInfo
(
	PileID int identity(1,1) primary key not null, --����
	PileName nvarchar(50) not null,
	PileCode nvarchar(50) not null unique, --ΨһԼ��
	StorageID int not null foreign key references StorageInfo(StorageID), --���
	PileSize nvarchar(50),
	Remark nvarchar(200)
)
--��ƽ���Z,�ڶ�λ�c�}��픵�����r��C�����̖"|"
go

create table DeliveredTypeInfo
(
	DeliveredTypeID int identity(1,1) primary key not null, --����
	DeliveredTypeName nvarchar(50) not null,
	DeliveredTypeCode nvarchar(50) not null unique, --ΨһԼ��

)
go
create table ReceivingTypeInfo
(
	ReceivingTypeID int identity(1,1) primary key not null, --����
	ReceivingTypeName nvarchar(50) not null,
	ReceivingTypeCode nvarchar(50) not null unique, --ΨһԼ��

)
go
--�����ʼ����,�ͻ�����ά��,�˱����ݲ���������
delete  from ReceivingTypeInfo
insert into ReceivingTypeInfo (ReceivingTypeName,ReceivingTypeCode) values('�������','01')
insert into ReceivingTypeInfo (ReceivingTypeName,ReceivingTypeCode) values('�������','02')

create table ProjectInfo
(
	ProjectID  int identity(1,1) primary key not null, --����
	Owner int not null foreign key references BusinessUnitInfo(BusinessUnitID),--��Ŀ����ҵ����λ
	ProjectName  nvarchar(50) not null,
	ProjectCode nvarchar(50) not null unique, --ΨһԼ��
	ProjectProperty   nvarchar(50) ,
	Remark nvarchar(200)
)
go
create table RelationProjectBusiness
(
	ProjectID int not null foreign key references ProjectInfo(ProjectID), -- ���
	BusinessUnitID int not null foreign key references BusinessUnitInfo(BusinessUnitID) Primary Key (ProjectID,BusinessUnitID)--���
)
go

create table MessageInfo--��Ϣ��Ϣ��
(
	MessageInfoID int identity(1,1) primary key not null,--����
	Creater int not null foreign key references EmpInfo(EmpID),--��Ϣ������
	MessageTitle nvarchar(50),--��Ϣ����
	MessageContent nvarchar(Max),--��Ϣ����
	MessageSource nvarchar(20) not null check(MessageSource in ('�������')),--��Ϣ���Ե�����
	MessageStatus nvarchar(10) not null check(MessageStatus in ('δ��','�Ѷ�')),
	MessageType nvarchar(10) not null check(MessageType in('������Ϣ','˽����Ϣ')),--��Ϣ���
	CreateTime datetime,--����ʱ��
	TaskID int not null--�����ĸ����񣬳�������/�������
	
)
go
create table MessageReceiver--��Ϣ�����ߣ�һ����Ϣ�����ж�������ߣ�
(
	MessageReceiverID int identity(1,1) primary key not null,--����
	MessageInfoID int not null foreign key references MessageInfo(MessageInfoID),--��Ϣ
	ReceiverID int not null foreign key references EmpInfo(EmpID)--��Ϣ������
)
go
create table LogInfo
(
	LogID int identity(1,1) primary key not null, --����
	LogType nvarchar(20) not null check(LogType in ('����','��Ϣ')),
	LogMessage nvarchar(max) not null,
	LogSource nvarchar(50) not null,
	LogUser int not null foreign key references EmpInfo(EmpID),
	LogDateTime datetime not null,
)
go



--****************************************************************************************
--��������޸ĺ����ݱ�
--Created By: Adonis
--Date:2010.10.18
--****************************************************************************************

--��Ҫ����
create table BatchOfIndex--���� 
(
	BatchOfIndexID int identity(1,1) primary key not null, --����
	BatchOfIndexName nvarchar(50) not null--������

)
go


create table StorageInMain --�������
(
	StorageInID int identity(1,1) primary key not null, --����
	StorageInCode nvarchar(50) not null,--����֪ͨ�����
    ReceivingType int foreign key references ReceivingTypeInfo(ReceivingTypeID),--�������	    
	Remark nvarchar(200),--��ע
	StorageInQualifiedNum nvarchar(50),--�ʼ�ϸ������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null--����ʱ��
)
go
create table StorageProduce--��������Ϣ��
(
	StorageInProduceID int identity(1,1) primary key not null, --����
	StorageInID int not null foreign key references StorageInMain(StorageInID),--������ⵥ,���
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--����
	QuantityGentaojian decimal(18,2),--��/̨/��/������
    QuantityMetre decimal(18,2), --�׵�����
    QuantityTon decimal(18,2),--�ֵ�����
    ExpectedProject int not null foreign key references ProjectInfo(ProjectID),--Ԥ��ʹ����Ŀ
	ExpectedTime datetime not null,--Ԥ�ڵ���ʱ��
	BatchIndex  nvarchar(50),--������Ϣ
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)--��ע
)
go


create table StorageInMaterials--��������Ϣ��
(
	StorageInMaterialsID int identity(1,1) primary key not null, --����
	ProduceID int not null foreign key references StorageProduce(StorageInProduceID),--������������Ϣ��,���
	RealGentaojian decimal(18,2),--��/̨/��/������
    RealMetre decimal(18,2), --�׵�����
    RealTon decimal(18,2),--�ֵ�����
	ManufacturerID int not null foreign key references Manufacturer(ManufacturerID),--��������ID,���
	IsManufacturer nvarchar(10) not null check(IsManufacturer in ('��','��')),--����������Ϣ�Ƿ���ɹ���ͬһ��
	SupplierID int not null foreign key references SupplierInfo(SupplierID),--��Ӧ��ID,���
	Supplier nvarchar(10) not null check(Supplier in ('��','��')),--��Ӧ����Ϣ�Ƿ���ɹ���ͬһ��
	Data nvarchar(10) not null check(Data in ('��','��')),--�����Ƿ���ȫ
	Standard nvarchar(10) not null check(Standard in ('��','��')),--�����׼�Ƿ���ɹ���ͬһ��
	Parts nvarchar(10) not null check(Parts in ('��','��')),--����Ƿ���ȫ
	Appearance nvarchar(10) not null check(Appearance in ('��','��')),--����Ƿ����
	PileID int not null foreign key references PileInfo(PileID),--�����ֿ�,������λ,���
	Creator int not null foreign key references EmpInfo(EmpID),--���ʹ���Ա,������
	StorageTime datetime not null,--ʵ�ʵ���ʱ��
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go

create table StorageInMaterialsLeader--�����鳤��˱�
(
	MaterialsLeaderID int identity(1,1) primary key not null, --����
	MaterialsID int not null foreign key references StorageInMaterials(StorageInMaterialsID),--������������Ϣ��,���
	Auditing nvarchar(10) not null check(Auditing in ('��','��')),--����Ƿ�ͨ��
	Auditingidea nvarchar(200),--������
	Creator int not null foreign key references EmpInfo(EmpID),--�����鳤,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע

)
go

create table StorageInTest--�ʼ���Ϣ��
(
	StorageInTestID int identity(1,1) primary key not null, --����
	MaterialsLeaderID int not null foreign key references StorageInMaterialsLeader(MaterialsLeaderID),--���������鳤��Ϣ��,���
	TestGentaojian decimal(18,2),--�ϸ��/̨/��/������
    TestMetre decimal(18,2), --�ϸ��׵�����
    TestTon decimal(18,2),--�ϸ�ֵ�����
	FailedGentaojian decimal(18,2),--�ʼ첻�ϸ��/̨/��/������
	FailedMetre decimal(18,2), --�ʼ첻�ϸ��׵�����
    FailedTon decimal(18,2),--�ʼ첻�ϸ�ֵ�����
	InspectionReportNum nvarchar(50) not null,--�������鱨���
	FileNameStr nvarchar(50) not null,--�ʼ챨���ĵ��ļ���
	Creator int not null foreign key references EmpInfo(EmpID),--�ʼ���Ա,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go


create table StorageInAssets--�ʲ�����Ϣ��
(
	StorageInAssetsID int identity(1,1) primary key not null, --����
	TestID int not null foreign key references StorageInTest(StorageInTestID),--������������Ϣ��,���
	BillCode nvarchar(50) not null,--��ⵥ�ݺ�
	financeCode nvarchar(50) not null,--������
	CurUnit nvarchar(50) check(CurUnit in ('��/̨/��/��','��','��')),--������λ
	UnitPrice decimal(18,2) not null,--����
	Amount decimal(18,2) not null,--���
	MaterialsAttribute nvarchar(50) not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--�ʲ���Ա,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go
create table StorageInHead--�ʲ��鳤��Ϣ��
(
	StorageInHeadID int identity(1,1) primary key not null, --����
	AssetsID int not null foreign key references StorageInAssets(StorageInAssetsID),--�����ʲ�����Ϣ��,���
	Auditing nvarchar(10) not null check(Auditing in ('��','��')),--����Ƿ�ͨ��
	Auditingidea nvarchar(200),--������
	Creator int not null foreign key references EmpInfo(EmpID),--�ʲ��鳤,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go
create table StorageDirector--������Ϣ��
(
	StorageInDirectorID int identity(1,1) primary key not null, --����
	HeadID int not null foreign key references StorageInHead(StorageInHeadID),--�����ʲ�����Ϣ��,���
	Approve nvarchar(10) not null check(Approve in ('��','��')),--�����Ƿ�ͨ��
	ApproveIdea nvarchar(200),--�������
	Creator int not null foreign key references EmpInfo(EmpID),--����,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go

create table TaskStorageIn --�û������б�
(
	TaskStorageID int identity(1,1) primary key not null, --����
	TaskCreaterID int not null foreign key references EmpInfo(EmpID),--�������� ���
	TaskTargetID int not null foreign key references EmpInfo(EmpID),--�������Ŀ�� ���
	StorageInType nvarchar(50) check(StorageInType in ('�������','ί�����','�������')),--������ͣ�1-������� 2-ί����� 3-�������
	StorageInID int not null,--�����������ⵥID
	QCBatch nvarchar(50),--�ʼ�����(ί�����������)
	TaskTitle nvarchar(50) not null,-- �������
	Remark nvarchar(200), --��ע
	InspectState nvarchar(50) check (InspectState in ('δ���','�����','ͨ��','����')),-- ���״̬
	TaskState nvarchar(50) check (TaskState in ('δ���','�����')),-- ����״̬
	TaskDispose nvarchar(50) default ('δ����') check(TaskDispose in ('δ����', '����')) ,--����״̬
	TaskType nvarchar(50) check(TaskType in ('������','������Ա','�����鳤','�ʼ�','�ʲ���Ա','�ʲ��鳤','�������','���������','�����鳤ȷ�������','�ʲ���������','������ⵥ�ʲ��鳤ȷ��','ά�ޱ��������鳤���','�����������','���������鳤�������','�����鰲���ʼ�','����Ա�ʼ�','�ʲ��鴦��ϸ�����','�ʲ��鳤ȷ�Ϻϸ�����','����������ά��','����Ա�����޸�����','�ʲ��鴦���޸��ϸ�����')),--�������ͣ�edit by adonis 2010-10-18 16��20
	PreviousTaskID int,--ǰ������,ͨ����ID���ҵ������ж�Ӧ�������Ϣ
	CreateTime datetime not null  --����ʱ��	
)
go


--create table TableOfStocks   --����
--(
	--StocksID int identity(1,1) primary key not null, --����
	----StorageInID int not null foreign key references StorageIn(StorageInID),--������ⵥ,���
	--StorageInID int not null,--������ⵥ
	--StorageInType nvarchar(50) check(StorageInType in ('�������','ί�����','�������')),--������ͣ�1-������� 2-ί����� 3-�������
	--MaterialID int not null foreign key references MaterialInfo(MaterialID),--���ϱ���
	--MaterialCode nvarchar(50),--���¿����ϱ��
	--SpecificationModel nvarchar(50) not null,--����ͺ�
	--UnitPrice decimal(18,2) not null,--����
	--NumberQualified decimal(18,2),--�ϸ�����(��ɾ���ֶΣ���Ϊ0)
	--Quantity decimal(18,2) not null,--��ǰ��ѡ��λ����
	--QuantityGentaojian decimal(18,2),--��/̨/��/������
    --QuantityMetre decimal(18,2), --�׵�����
    --QuantityTon decimal(18,2),--�ֵ�����
    --CurUnit nvarchar(50) check(CurUnit in ('��/̨/��/��','��','��')),--��ǰ������λ
    --PileID int not null foreign key references PileInfo(PileID),--������λ,���
	--financeCode nvarchar(50) not null,--������
	--StorageTime datetime not null,--����ʱ��
	--SupplierID int not null foreign key references SupplierInfo(SupplierID),--��Ӧ��,���
	--MaterialsManager int not null foreign key references EmpInfo(EmpID),--���ʹ���Ա
	--WarehouseWorker int not null foreign key references EmpInfo(EmpID),--�ֿ�Ա
	--OnlineState nvarchar(50) default ('����') check(OnlineState in ('����', '����')) ,--����״̬
	----OnlineCode nvarchar(50)
	--Remark nvarchar(200)
--)
--go


create table TableOfStocks   --����
(
	StocksID int identity(1,1) primary key not null, --����
	StorageInID int,--������ⵥ(����֪ͨ�����)
	StorageInType nvarchar(50) check(StorageInType in ('�������','ί�����','�������')),--������ͣ�1-������� 2-ί����� 3-�������
	
	ReceivingTypeName  nvarchar(50),--�����������
	StorageInCode nvarchar(50),--���֪ͨ����
	BillCode nvarchar(50),--��ⵥ��(CommitInAssets)
	
	MaterialID int foreign key references MaterialInfo(MaterialID),--���ϱ���(��������,����ͺ�,�������)
	MaterialCode nvarchar(50),--���¿����ϱ��
	QuantityGentaojian decimal(18,2),--��/̨/��/������
    QuantityMetre decimal(18,2), --�׵�����
    QuantityTon decimal(18,2),--�ֵ�����
	CurUnit nvarchar(50) check(CurUnit in ('��/̨/��/��','��','��')),--��ǰ������λ
	UnitPrice decimal(18,2) not null,--����
	Amount decimal(18,2) not null,--���
	ExpectedProject int foreign key references ProjectInfo(ProjectID),--Ԥ��ʹ����Ŀ
	Remark nvarchar(max), -- ��������
	BatchIndex  nvarchar(50),--������Ϣ
	ManufacturerID int  foreign key references Manufacturer(ManufacturerID),--��������,���
	SupplierID int foreign key references SupplierInfo(SupplierID),--��Ӧ��,���
	StorageID int foreign key references StorageInfo(StorageID),--���ڲֿ�
	PileID int foreign key references PileInfo(PileID),--������λ
	MaterialsManager int foreign key references EmpInfo(EmpID),--���ʹ���Ա
	AssetsManager int foreign key references EmpInfo(EmpID),--�ʲ�����Ա
	StorageTime datetime not null,--ʵ�ʵ���ʱ��
	Creator int foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null  --����ʱ��
)
go

create table StockOnline   --�������ϱ�(���Ͽ��)
(
	StockOnlineID  int identity(1,1) primary key not null, --����
	--TableOfStocksID int not null foreign key references TableOfStocks(StocksID),--�������,���
	
	
	StorageInID int,--������ⵥ(����֪ͨ�����)
	StorageInType nvarchar(50) check(StorageInType in ('�������','ί�����','�������')),--������ͣ�1-������� 2-ί����� 3-�������
	ReceivingTypeName  nvarchar(50),--�����������
	StorageInCode nvarchar(50),--���֪ͨ����
	BillCode nvarchar(50),--��ⵥ��(CommitInAssets)
	MaterialID int foreign key references MaterialInfo(MaterialID),--���ϱ���(��������,����ͺ�,�������)
	MaterialCode nvarchar(50),--���¿����ϱ��
	OfflineGentaojian decimal(18,2),--��/̨/��/������
    OfflineMetre decimal(18,2), --�׵�����
    OfflineTon decimal(18,2),--�ֵ�����
	CurUnit nvarchar(50) check(CurUnit in ('��/̨/��/��','��','��')),--��ǰ������λ
	UnitPrice decimal(18,2) not null,--����
	Amount decimal(18,2) not null,--���
	ExpectedProject int foreign key references ProjectInfo(ProjectID),--Ԥ��ʹ����Ŀ
	Remark nvarchar(max), -- ��������
	BatchIndex  nvarchar(50),--������Ϣ
	ManufacturerID int  foreign key references Manufacturer(ManufacturerID),--��������,���
	SupplierID int foreign key references SupplierInfo(SupplierID),--��Ӧ��,���
	StorageID int foreign key references StorageInfo(StorageID),--���ڲֿ�
	PileID int foreign key references PileInfo(PileID),--������λ
	MaterialsManager int foreign key references EmpInfo(EmpID),--���ʹ���Ա
	AssetsManager int foreign key references EmpInfo(EmpID),--�ʲ�����Ա
	StorageTime datetime not null,--ʵ�ʵ���ʱ��
	
	OrderNum nvarchar(50),--�ɹ�������
	CertificateNum nvarchar(50),--����ƾ֤��
	OnlineCode nvarchar(50),--�������ϱ��
	OnlineUnit nvarchar(50) check(OnlineUnit in ('��/̨/��/��','��','��')),--�������ʼ�����λ
	QuantityGentaojian decimal(18,2),--��/̨/��/������
	QuantityMetre decimal(18,2), --�׵�����
    QuantityTon decimal(18,2),--�ֵ�����
	CurQuantity decimal(18,2),--��ǰ��λ����
	OnlineUnitPrice decimal(18,2), --�������ϵ���,���/����
	OnlineTotal decimal(18,2), -- �������Ͻ��
	OnlineDate datetime, --��������ʱ��
	
	Creator int  foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null  --����ʱ��
)
go


create table  FlowDetailsOffline   --���������(���¿��)
(
	FlowDetailsID  int identity(1,1) primary key not null, --����
	TableOfStocksID int not null foreign key references TableOfStocks(StocksID),--�������,���
	StorageType nvarchar(50) check(StorageType in ('��������','ί�����')),--������ͣ�1-�������� 2-ί����� 
	StorageOutCode nvarchar(50) not null,--���������
	StorageOutProject int not null foreign key references ProjectInfo(ProjectID),--���ⵥ������Ŀ
	CurUnit nvarchar(50) check(CurUnit in ('��/̨/��/��','��','��')),--��ǰ������λ
	RealGentaojian decimal(18,2) not null,--��/̨/��/������(��ע)
    RealMetre decimal(18,2) not null, --�׵�����(��ע)
    RealTon decimal(18,2) not null,--�ֵ�����(��ע)
    CurQuantity decimal(18,2) not null,--��ǰ��λ���������� 
    Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null  --����ʱ��
)




--*********************************************************
--������������������ݱ�
--*********************************************************

go

create table StorageOutTask --���������û������б�
(
	TaskID int identity(1,1) primary key not null, --����
	Process nvarchar(50) check(Process in ('��������','ί�����')),--��������
	TaskCreaterID int not null foreign key references EmpInfo(EmpID),--�������� ���
	TaskTargetID int not null foreign key references EmpInfo(EmpID),--�������Ŀ�� ���	
	NoticeID int not null,--�漰����֪ͨ��	
	TaskTitle nvarchar(50) not null,-- �������
	Remark nvarchar(200), --��ע
	TaskState nvarchar(50) check (TaskState in ('δ���','�����')),-- ����״̬
	TaskDispose nvarchar(50) default ('δ����') check(TaskDispose in ('δ����', '����')) ,--����״̬
	TaskType nvarchar(50) check(TaskType in ('���ʵ��������Ϣ','���ʳ��������Ϣ','���ʵ������','���ʳ���','���ʳ������','��������')),
	CreateTime datetime not null,  --����ʱ��
	PreviousTaskID int not null--ǰ������
)

go

create table StorageOutNotice        --�����豸����֪ͨ��
(
	StorageOutNoticeID int identity(1,1) primary key not null,--����
	StorageOutNoticeCode nvarchar(50) not null unique, --����֪ͨ�����
	ProjectStage nvarchar(20) not null check(ProjectStage in ('�꾮','�꾮','����','���潨��','����')),
	ProjectID int not null foreign key references ProjectInfo(ProjectID),--��Ŀ
	Proprietor int not null foreign key references BusinessUnitInfo(BusinessUnitID),--ҵ����λ
	Constructor int not null foreign key references BusinessUnitInfo(BusinessUnitID),--ʩ����λ
	Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null,--��������
	Remark nvarchar(200)
)

go

create table StorageOutDetails    --���ʵ�����ϸ��
(
	StorageOutDetailsID int identity(1,1) primary key not null,--����
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID) on delete cascade,--��������֪ͨ��	
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--����	
	Gentaojian decimal(18,2),--��/̨/��/������
    Metre decimal(18,2), --�׵�����
    Ton decimal(18,2),--�ֵ�����	
	CreateTime datetime not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)	
)

go
create table StorageOutProduceAudit   --���������鳤��˱�
(
	StorageOutProduceAuditID int identity(1,1) primary key not null,--����
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID),--����֪ͨ��
	AuditStatus nvarchar(10) not null,--���״̬(ͨ��,δͨ��)
	AuditOpinion nvarchar(200),--������
	AuditTime datetime not null,--���ʱ��
	ProduceChief int not null foreign key references EmpInfo(EmpID),--�����鳤
	TaskID int not null foreign key references StorageOutTask(TaskID)--�����ĸ�����		
)

go
create table StorageOutRealDetails    --������ϸ��
(
	StorageOutRealDetailsID int identity(1,1) primary key not null,--����
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID),--��������֪ͨ��
	StorageOutDetailsID int not null foreign key references StorageOutDetails(StorageOutDetailsID),--��Ӧ�ĵ�������
	StocksID int not null,--����������ID
	MaterialStatus nvarchar(10) not null check(MaterialStatus in('����','����')),
	RealGentaojian decimal(18,2) not null,--��/̨/��/������
    RealMetre decimal(18,2) not null, --�׵�����
    RealTon decimal(18,2) not null,--�ֵ�����	
	RealAmount decimal(18,2) not null,--ʵ�ʽ��
	CreateTime datetime not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)	
)

go
create table StorageOutAssetAudit --�����ʲ��鳤��˱�
(
	StorageOutAssetAuditID int identity(1,1) primary key not null,--����
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID),--����֪ͨ����
	StorageOutProduceAuditID int not null foreign key references StorageOutProduceAudit(StorageOutProduceAuditID),--���������鳤���
	AuditStatus nvarchar(10) not null,--���״̬ 
	AuditOpinion nvarchar(200),--������		
	AuditTime datetime not null,--���ʱ��
	AssetChief int not null foreign key references EmpInfo(EmpID),--�ʲ��鳤
	TaskID int not null foreign key references StorageOutTask(TaskID)--�����ĸ�����
)
go

create table StorageOutDirectorConfirm --��������ȷ��
(
	StorageOutDirectorConfirmID int identity(1,1) primary key not null,--����
	StorageOutNoticeID int not null foreign key references StorageOutNotice(StorageOutNoticeID),--����֪ͨ����	
	StorageOutAssetAuditID int not null foreign key references StorageOutAssetAudit(StorageOutAssetAuditID),--�ʲ��鳤���ID	
	ConfirmTime datetime not null,--�������ʱ��
	Director int not null foreign key references EmpInfo(EmpID),--����
	TaskID int not null foreign key references StorageOutTask(TaskID)--�����ĸ�����		
)
go

--**************************************************************
--���δ���
--**************************************************************
create table TaskProxyType --ί����������
(
	TaskProxyTypeID int identity(1,1) primary key not null, --����
	TaskProxyTypeName nvarchar(50) not null--ί��������������
)
go

create table TaskProxy--���δ��������
(
	TaskProxyID  int identity(1,1) primary key not null, --����
	ProxyPrincipal int not null foreign key references EmpInfo(EmpID),--ί����
	ProxyFiduciary int not null foreign key references EmpInfo(EmpID), --������
	ProxyTaskType  int not null foreign key references TaskProxyType(TaskProxyTypeID),-- ί����������
	StartTime datetime not null,--����ʼ����
	EndTime datetime not null,--�����������
	CreateTime datetime not null,--��������
	TaskDispose nvarchar(50) default ('������') check(TaskDispose in ('������','������','�ѹ���')) ,--����״̬
	Remark nvarchar(200)--��ע
)
go
create table ProxyDirector --�����������ϵ��
(
	ProxyDirectorID int identity(1,1) primary key not null, --����
	TaskID int not null,--����ID
	TaskProxyID  int not null foreign key references TaskProxy(TaskProxyID),--��������ID
)
go

--*********************************************************
--ί���������������ݱ�
--Created By: Xu Chun Lei
--Date:2010.07.05-2010.07.06
--*********************************************************

create table StorageCommitOutNotice        --�����豸ί�����֪ͨ��
(
	StorageCommitOutNoticeID int identity(1,1) primary key not null,--����
	StorageCommitOutNoticeCode nvarchar(50) not null unique, --����֪ͨ�����
	Receiver int not null foreign key references BusinessUnitInfo(BusinessUnitID),--���ϵ�λ
	Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null,--��������
	Remark nvarchar(200)
)

go

create table StorageCommitOutDetails    --ί�����ʵ�����ϸ��
(
	StorageCommitOutDetailsID int identity(1,1) primary key not null,--����
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID) on delete cascade,--��������֪ͨ��	
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--����	
	Gentaojian decimal(18,2),--��/̨/��/������
    Metre decimal(18,2), --�׵�����
    Ton decimal(18,2),--�ֵ�����	
	CreateTime datetime not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)	
)

go
create table StorageCommitOutProduceAudit   --ί����������鳤��˱�
(
	StorageCommitOutProduceAuditID int identity(1,1) primary key not null,--����
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID),--����֪ͨ��
	AuditStatus nvarchar(10) not null,--���״̬(ͨ��,δͨ��)
	AuditOpinion nvarchar(200),--������
	AuditTime datetime not null,--���ʱ��
	ProduceChief int not null foreign key references EmpInfo(EmpID),--�����鳤
	TaskID int not null foreign key references StorageOutTask(TaskID)--�����ĸ�����		
)

go
create table StorageCommitOutRealDetails    --ί�������ϸ��
(
	StorageCommitOutRealDetailsID int identity(1,1) primary key not null,--����
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID),--��������֪ͨ��
	StorageCommitOutDetailsID int not null foreign key references StorageCommitOutDetails(StorageCommitOutDetailsID),--��Ӧ�ĵ�������
	StocksID int not null,--����������ID
	MaterialStatus nvarchar(10) not null check(MaterialStatus in('����','����')),
	RealGentaojian decimal(18,2) not null,--��/̨/��/������
    RealMetre decimal(18,2) not null, --�׵�����
    RealTon decimal(18,2) not null,--�ֵ�����	
	RealAmount decimal(18,2) not null,--ʵ�ʽ��
	CreateTime datetime not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)	
)

go
create table StorageCommitOutAssetAudit --ί������ʲ��鳤��˱�
(
	StorageCommitOutAssetAuditID int identity(1,1) primary key not null,--����
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID),--����֪ͨ����
	StorageCommitOutProduceAuditID int not null foreign key references StorageCommitOutProduceAudit(StorageCommitOutProduceAuditID),--���������鳤���
	AuditStatus nvarchar(10) not null,--���״̬ 
	AuditOpinion nvarchar(200),--������		
	AuditTime datetime not null,--���ʱ��
	AssetChief int not null foreign key references EmpInfo(EmpID),--�ʲ��鳤
	TaskID int not null foreign key references StorageOutTask(TaskID)--�����ĸ�����
)
go

create table StorageCommitOutDirectorConfirm --ί���������ȷ��
(
	StorageCommitOutDirectorConfirmID int identity(1,1) primary key not null,--����
	StorageCommitOutNoticeID int not null foreign key references StorageCommitOutNotice(StorageCommitOutNoticeID),--����֪ͨ����	
	StorageCommitOutAssetAuditID int not null foreign key references StorageCommitOutAssetAudit(StorageCommitOutAssetAuditID),--�ʲ��鳤���ID	
	ConfirmTime datetime not null,--�������ʱ��
	Director int not null foreign key references EmpInfo(EmpID),--����
	TaskID int not null foreign key references StorageOutTask(TaskID)--�����ĸ�����		
)
go

--*********************************************************
--ί�����������ݱ�(add by adonis)
--*********************************************************




create table CommitInMain --�������
(
	StorageInID int identity(1,1) primary key not null, --����
	StorageInCode nvarchar(50) not null,--����֪ͨ�����
    ReceivingType nvarchar(50) not null,--�������	    
	Remark nvarchar(200),--��ע
	StorageInQualifiedNum nvarchar(50),--�ʼ�ϸ������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null--����ʱ��
)
go
create table CommitProduce--��������Ϣ��
(
	StorageInProduceID int identity(1,1) primary key not null, --����
	StorageInID int not null foreign key references CommitInMain(StorageInID),--������ⵥ,���
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--����
	QuantityGentaojian decimal(18,2),--��/̨/��/������
    QuantityMetre decimal(18,2), --�׵�����
    QuantityTon decimal(18,2),--�ֵ�����
    ExpectedProject int not null foreign key references ProjectInfo(ProjectID),--Ԥ��ʹ����Ŀ
	ExpectedTime datetime not null,--Ԥ�ڵ���ʱ��
	BatchIndex  nvarchar(50),--������Ϣ
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)--��ע
)
go


create table CommitInMaterials--��������Ϣ��
(
	StorageInMaterialsID int identity(1,1) primary key not null, --����
	ProduceID int not null foreign key references CommitProduce(StorageInProduceID),--������������Ϣ��,���
	RealGentaojian decimal(18,2),--��/̨/��/������
    RealMetre decimal(18,2), --�׵�����
    RealTon decimal(18,2),--�ֵ�����
	ManufacturerID int not null foreign key references Manufacturer(ManufacturerID),--��������ID,���
	IsManufacturer nvarchar(10) not null check(IsManufacturer in ('��','��')),--����������Ϣ�Ƿ���ɹ���ͬһ��
	SupplierID int not null foreign key references SupplierInfo(SupplierID),--��Ӧ��ID,���
	Supplier nvarchar(10) not null check(Supplier in ('��','��')),--��Ӧ����Ϣ�Ƿ���ɹ���ͬһ��
	Data nvarchar(10) not null check(Data in ('��','��')),--�����Ƿ���ȫ
	Standard nvarchar(10) not null check(Standard in ('��','��')),--�����׼�Ƿ���ɹ���ͬһ��
	Parts nvarchar(10) not null check(Parts in ('��','��')),--����Ƿ���ȫ
	Appearance nvarchar(10) not null check(Appearance in ('��','��')),--����Ƿ����
	PileID int not null foreign key references PileInfo(PileID),--�����ֿ�,������λ,���
	Creator int not null foreign key references EmpInfo(EmpID),--���ʹ���Ա,������
	StorageTime datetime not null,--ʵ�ʵ���ʱ��
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go

create table CommitInMaterialsLeader--�����鳤��˱�
(
	MaterialsLeaderID int identity(1,1) primary key not null, --����
	MaterialsID int not null foreign key references CommitInMaterials(StorageInMaterialsID),--������������Ϣ��,���
	Auditing nvarchar(10) not null check(Auditing in ('��','��')),--����Ƿ�ͨ��
	Auditingidea nvarchar(200),--������
	Creator int not null foreign key references EmpInfo(EmpID),--�����鳤,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע

)
go

create table CommitInTest--�ʼ���Ϣ��
(
	StorageInTestID int identity(1,1) primary key not null, --����
	MaterialsLeaderID int not null foreign key references CommitInMaterialsLeader(MaterialsLeaderID),--���������鳤��Ϣ��,���
	TestGentaojian decimal(18,2),--�ϸ��/̨/��/������
    TestMetre decimal(18,2), --�ϸ��׵�����
    TestTon decimal(18,2),--�ϸ�ֵ�����
	FailedGentaojian decimal(18,2),--�ʼ첻�ϸ��/̨/��/������
	FailedMetre decimal(18,2), --�ʼ첻�ϸ��׵�����
    FailedTon decimal(18,2),--�ʼ첻�ϸ�ֵ�����
	InspectionReportNum nvarchar(50) not null,--�������鱨���
	FileNameStr nvarchar(50) not null,--�ʼ챨���ĵ��ļ���
	Creator int not null foreign key references EmpInfo(EmpID),--�ʼ���Ա,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go


create table CommitInAssets--�ʲ�����Ϣ��
(
	StorageInAssetsID int identity(1,1) primary key not null, --����
	TestID int not null foreign key references CommitInTest(StorageInTestID),--������������Ϣ��,���
	BillCode nvarchar(50) not null,--��ⵥ�ݺ�
	financeCode nvarchar(50) not null,--������
	CurUnit nvarchar(50) check(CurUnit in ('��/̨/��/��','��','��')),--������λ
	UnitPrice decimal(18,2) not null,--����
	Amount decimal(18,2) not null,--���
	MaterialsAttribute nvarchar(50) not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--�ʲ���Ա,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go
create table CommitInHead--�ʲ��鳤��Ϣ��
(
	StorageInHeadID int identity(1,1) primary key not null, --����
	AssetsID int not null foreign key references CommitInAssets(StorageInAssetsID),--�����ʲ�����Ϣ��,���
	Auditing nvarchar(10) not null check(Auditing in ('��','��')),--����Ƿ�ͨ��
	Auditingidea nvarchar(200),--������
	Creator int not null foreign key references EmpInfo(EmpID),--�ʲ��鳤,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go
create table CommitDirector--������Ϣ��
(
	StorageInDirectorID int identity(1,1) primary key not null, --����
	HeadID int not null foreign key references CommitInHead(StorageInHeadID),--�����ʲ�����Ϣ��,���
	Approve nvarchar(10) not null check(Approve in ('��','��')),--�����Ƿ�ͨ��
	ApproveIdea nvarchar(200),--�������
	Creator int not null foreign key references EmpInfo(EmpID),--����,������
    CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)--��ע
)
go


create table RelationCommitIn --ί�������ί������ϵ��
(
	RelationID int identity(1,1) primary key not null, --����
	CommitMaterial int not null foreign key references CommitProduce(StorageInProduceID),--�½�ί������
    CommitOutMaterial  nvarchar(50) not null,--�½�������Դ��ί���������(�����Ƕ��,StorageCommitOutRealDetails��id)
	CreateTime datetime not null--����ʱ��
)
go





--*********************************************************
--�ƿ�����������ݱ�
--*********************************************************

create table StockTransfer   -- �ƿ�����
(
	StockTransferID int identity(1,1) primary key not null,--����
	StockTransferNum nvarchar(50) not null, --�������
	CreateTime  datetime not null ,--����ʱ��	
	Creater int not null  foreign key references EmpInfo(EmpID), --���񴴽���
	Remark nvarchar(200)	
)
go
create table StockTransferTask --�������ݼ�¼��
(
	StockTransferTaskID int identity(1,1) primary key not null,--����
	StockTransferID int not null ,
	TaskCreaterID int foreign key references EmpInfo(EmpID),--�������� ���
	TaskTargetID int  foreign key references EmpInfo(EmpID),--�������Ŀ�� ���
	TaskInType  nvarchar(50) check(TaskInType in('�ƿ�����')),--��������	
	TaskTitle nvarchar(50) not null, --�������
	AcceptTime datetime, --ͨ���r�g
	AuditOpinion nvarchar(200),--������
	AuditStatus nvarchar(50) default ('δ���') check(AuditStatus in ('δ���', '���ͨ��','���δͨ��')) ,--���״̬(δ���,���ͨ��,���δͨ��)
	TaskState nvarchar(50) check (TaskState in ('δ���','�����')),-- ����״̬
	TaskDispose nvarchar(50) default ('δ����') check(TaskDispose in ('δ����', '����')) ,--����״̬
	TaskType nvarchar(50) check(TaskType in ('�����鳤�����Ϣ','�������޸�')),
	CreateTime  datetime not null ,--����ʱ��	
	Remark nvarchar(200)	
)
go
create table StockTransferDetail --���ƿ�����������嵥
(
	StockTransferDetailID  int identity(1,1) primary key not null,--����
	StockTransferID  int not null,
	DetailType  nvarchar(50) check(DetailType in('�ƿ�����')),--��������	
	StocksID int not null,--����������
	StocksStatus nvarchar(50)check(StocksStatus in('����','����','���պϸ�')),--����״̬,��stocksid ������Ψһ��ʶ
	--Quantity decimal(18,2) not null,--��������
	TargetPile int not null foreign key references PileInfo(PileID), --��Ҫ�Ƅӵ��Ķ�λ		
	QuantityGentaojian decimal(18,2),--��/̨/��/������
    QuantityMetre decimal(18,2), --�׵�����
    QuantityTon decimal(18,2),--�ֵ�����
	Remark nvarchar(200)	
)
go

--*********************************************************
--�����������������ݱ�
--Created By: Xu Chun Lei
--Date:2010.07.26-2010.08.19
--*********************************************************
create table SrinSubDoc--���շֵ��������齫һ���󵥻���Ϊ���ɷֵ���
(
	SrinSubDocID int identity(1,1) primary key not null,--����	
	Project int not null foreign key references ProjectInfo(ProjectID),--������Ŀ
	Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null,--��������
	Taker int not null foreign key references EmpInfo(EmpID),--�н���	
	Remark nvarchar(200)
)

go

create table SrinSubDetails--���շֵ���ϸ-����������
(
	SrinSubDetailsID int identity(1,1) primary key not null,--����
	SrinSubDocID int not null foreign key references SrinSubDoc(SrinSubDocID) on delete cascade,--���շֵ�	
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--����	
	TotleGentaojian decimal(18,2),--��/̨/��/������
    TotleMetre decimal(18,2), --�׵�����
    TotleTon decimal(18,2),--�ֵ�����   
    RetrieveCode nvarchar(50),--���յ���
	CreateTime datetime not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)
)
go

create table SrinStocktaking--���ʹ���Ա����
(
	SrinStocktakingID int identity(1,1) primary key not null,--����
	SrinSubDocID int not null foreign key references SrinSubDoc(SrinSubDocID),--���Ļ��շֵ�
	StocktakingResult nvarchar(10) not null check(StocktakingResult in('�������','�������')),--�����
	StocktakingDate datetime not null,--�������	
	StocktakingProblem nvarchar(max),--�������
	Creator int not null foreign key references EmpInfo(EmpID),--�����
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--�����ĸ�����
)
go

create table SrinStocktakingDetails--���������ϸ
(
	SrinStocktakingDetailsID int identity(1,1) primary key not null,--����
	SrinSubDetailsID int not null foreign key references SrinSubDetails(SrinSubDetailsID),--�����ĸ����շֵ�
	SrinStocktakingID int not null foreign key references SrinStocktaking(SrinStocktakingID),--����������嵥
	StorageID int foreign key references StorageInfo(StorageID),--���ڲֿ�
	PileID int foreign key references PileInfo(PileID),--������λ
	CreateTime datetime not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)
)

go

create table SrinStocktakingConfirm--�����鳤ȷ������
(
	SrinStocktakingConfirmID int identity(1,1) primary key not null,--����
	SrinStocktakingID int not null foreign key references SrinStocktaking(SrinStocktakingID),--ȷ�ϵ�����嵥
	MaterialChief int not null foreign key references EmpInfo(EmpID),--�����鳤
	ConfirmTime datetime not null,--��������
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID),--����������
)

go
create table SrinReceipt--���������豸��ⵥ(�ܵ�)
(
	SrinReceiptID int identity(1,1) primary key not null, --����
	SrinStocktakingConfirmID int not null foreign key references SrinStocktakingConfirm(SrinStocktakingConfirmID),--���Ե�����ĵ���	
	SrinReceiptCode nvarchar(50) not null unique,--����������ⵥ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null,--��������
	TaskID int foreign key references TaskStorageIn(TaskStorageID),--�������ݵ�����
	Remark nvarchar(200)
)
go

create table SrinDetails--���������豸�������
(
	SrinDetailsID int identity(1,1) primary key not null,--����
	SrinStocktakingDetailsID int not null foreign key references SrinStocktakingDetails(SrinStocktakingDetailsID),--��Ӧ���������
	SrinReceiptID int not null foreign key references SrinReceipt(SrinReceiptID),--����������ⵥ
	CurUnit nvarchar(50) not null check(CurUnit in ('��/̨/��/��','��','��')),--��ǰ������λ
    UnitPrice decimal(18,2) not null,--����	
	Amount decimal(18,2) not null,--���		
	CreateTime datetime not null,--��������
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)
	
)	

go

create table SrinAssetReceiptConfirm--���������豸��ⵥ(�ܵ�)�ʲ��鳤ȷ��
(
	SrinAssetReceiptConfirmID int identity(1,1) primary key not null,--����
	SrinReceiptID int not null foreign key references SrinReceipt(SrinReceiptID),--�����Ļ�����ⵥ
	MaterialChief int not null foreign key references EmpInfo(EmpID),--�����鳤
	ConfirmTime datetime not null,--��������
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID),--����������
)

go

create table SrinRepairPlan--ά�ޱ����ƻ���
(
	SrinRepairPlanID int identity(1,1) primary key not null,--����
	SrinReceiptID int not null foreign key references SrinReceipt(SrinReceiptID),--����������ⵥ
	SrinRepairPlanCode nvarchar(50) not null unique,--ά�ޱ����ƻ�����
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID),--�����ĸ�����
)

go
create table SrinMaterialRepairDetails--ά�ޱ�������ϸ--���ʹ���Ա
(
	SrinMaterialRepairDetailsID int identity(1,1) primary key not null,--����
	SrinRepairPlanID int not null foreign key references SrinRepairPlan(SrinRepairPlanID) on delete cascade,--����ά�ޱ����ƻ���
	SrinDetailsID int not null foreign key references SrinDetails(SrinDetailsID),--�����������
	Gentaojian decimal(18,2) not null,--ά�ޱ�������
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--��������,���
	ArrivalTime datetime not null,--����ʱ��
	RepairReason nvarchar(200),--ά��ԭ��
	PlanTime datetime,--�ƻ����ʱ��
	RealTime datetime,--ʵ�����ʱ��
	RealGentaojian decimal(18,2) not null,--ʵ��ά�ޱ�������
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID)--������
	
)
go

create table SrinMaterialRepairAudit--ά�ޱ�����--�����鳤���
(
	SrinMaterialRepairAuditID int identity(1,1) primary key not null,--����
	SrinRepairPlanID int not null foreign key references SrinRepairPlan(SrinRepairPlanID),--����˵�ά�ޱ����ƻ���
	AuditResult nvarchar(10) not null check(AuditResult in('ͨ��','δͨ��')),--���״̬(ͨ��,δͨ��)
	AuditOpinion nvarchar(200),--������
	AuditTime datetime not null,--���ʱ��
	MaterialChief int not null foreign key references EmpInfo(EmpID),--�����鳤	
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--�����ĸ�����
)

go
create table SrinVerifyTransfer--�������ʼ��鴫�ݱ�
(
	SrinVerifyTransferID int identity(1,1) primary key not null,--����
	SrinReceiptID int not null foreign key references SrinReceipt(SrinReceiptID),--����������ⵥ
	SrinVerifyTransferCode nvarchar(50) not null unique,--�������ʼ��鴫�ݱ���
	ReadyWorkIsFinished bit not null default(1),--׼�������Ƿ����
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--�����ĸ�����
)
go
create table SrinMaterialVerifyDetails--���ռ�������--���ʹ���Ա
(
	SrinMaterialVerifyDetailsID int identity(1,1) primary key not null,--����
	SrinVerifyTransferID int not null foreign key references SrinVerifyTransfer(SrinVerifyTransferID) on delete cascade,--�����������ʼ��鴫�ݱ�
	SrinDetailsID int not null foreign key references SrinDetails(SrinDetailsID),--�����������	
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--��������,���
	RetrieveTime datetime not null,--����ʱ��
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID)--������	
)

go

create table SrinProduceVerifyTransfer--�������ʼ��鴫�ݱ�-������ȷ�����ʼ�ʱ���
(
	SrinProduceVerifyTransferID int identity(1,1) primary key not null,--����
	SrinVerifyTransferID int not null foreign key references SrinVerifyTransfer(SrinVerifyTransferID),--�����Ļ������ʼ��鴫�ݱ�
	VerifyTime datetime not null,
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--�����ĸ�����
)
go

create table SrinInspectorVerifyTransfer--�������ʼ��鴫�ݱ�-�ʼ���Ա�ʼ�֮��
(
	SrinInspectorVerifyTransferID int identity(1,1) primary key not null,--����
	SrinProduceVerifyTransferID int not null foreign key references SrinProduceVerifyTransfer(SrinProduceVerifyTransferID),--�����Ļ������ʼ��鴫�ݱ�	
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--�����ĸ�����
)
go

create table SrinInspectorVerifyDetails--���ռ�������-�ʼ���
(
	SrinInspectorVerifyDetailsID int identity(1,1) primary key not null,--����
	SrinInspectorVerifyTransferID int not null foreign key references SrinInspectorVerifyTransfer(SrinInspectorVerifyTransferID),--���ռ��鴫�ݱ�
	SrinMaterialVerifyDetailsID int not null foreign key references SrinMaterialVerifyDetails(SrinMaterialVerifyDetailsID),--���������ʹ���Ա���ռ�������
	QualifiedGentaojian decimal(18,2) not null,--�ϸ�����
	RepairGentaojian decimal(18,2) not null,--��ά������
	RejectGentaojian decimal(18,2) not null,--����������
	VerifyCode nvarchar(50),--�ʼ챨���
	RealVerifyTime datetime not null,--ʵ���ʼ�ʱ��
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID)--������
)

go

create table SrinQualifiedReceipt--���������豸���ϸ���ⵥ
(
	SrinQualifiedReceiptID int identity(1,1) primary key not null, --����
	SrinInspectorVerifyTransferID int not null foreign key references SrinInspectorVerifyTransfer(SrinInspectorVerifyTransferID),--�����Ļ������ʼ��鴫�ݱ�		
	SrinQualifiedReceiptCode nvarchar(50) not null unique,--����������ⵥ��
	NeedWriteOff bit not null,--�Ƿ���Ҫ����
	Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null,--��������
	TaskID int foreign key references TaskStorageIn(TaskStorageID),--�������ݵ�����
	Remark nvarchar(200)
)

go
create table SrinAssetQualifiedDetails--������ⵥ���ϸ�����--�ʲ�����Ա
(
	SrinAssetQualifiedDetailsID int identity(1,1) primary key not null,--����
	SrinQualifiedReceiptID int not null foreign key references SrinQualifiedReceipt(SrinQualifiedReceiptID),--���ռ��鴫�ݱ�
	SrinInspectorVerifyDetailsID int not null foreign key references SrinInspectorVerifyDetails(SrinInspectorVerifyDetailsID),--��Ӧ���ʼ�����
	Gentaojian decimal(18,2) not null,--�ϸ�����
	Metre decimal(18,2) not null,--������
	Ton decimal(18,2) not null,--������
	Amount decimal(18,2) not null,--���
	OutUnitPrice decimal(18,2) not null,--���ⵥ��(ԭ)
	InUnitPrice decimal(18,2) not null,--��ⵥ��(��)
	CurUnit nvarchar(50) not null check(CurUnit in ('��/̨/��/��','��','��')),--������λ
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--��������,���
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID)--������
)
go

create table SrinAChiefQReceiptConfirm--������ⵥ���ϸ��ʲ��鳤ȷ��
(
	SrinAChiefQReceiptConfirmID int identity(1,1) primary key not null,--����
	SrinQualifiedReceiptID int not null foreign key references SrinQualifiedReceipt(SrinQualifiedReceiptID),--�����Ļ�����ⵥ
	AssetChief int not null foreign key references EmpInfo(EmpID),--�ʲ��鳤
	ConfirmTime datetime not null,--��������
	Remark nvarchar(200),
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--����������
)
go

create table QualifiedStocks--���պϸ����ʿ�
(
	StocksID int identity(1,1) primary key not null,--����
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--������Ϣ���������ơ�����ͺš��������
	StorageTime datetime not null,--����ʱ��
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--��������
	StorageID int not null foreign key references StorageInfo(StorageID),--���ڲֿ�
	PileID int not null foreign key references PileInfo(PileID),--������λ
	Gentaojian decimal(18,2) not null,--�ϸ�����
	Metre decimal(18,2) not null,--������
	Ton decimal(18,2) not null,--������
	CurUnit nvarchar(50) not null check(CurUnit in ('��/̨/��/��','��','��')),--������λ
	UnitPrice decimal(18,2) not null,--����
	Amount decimal(18,2) not null,--���
	RetrieveTime datetime not null,--����ʱ��
	RetrieveProjectID int not null foreign key references ProjectInfo(ProjectID),--������Ŀ
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID)--������
)

create table SrinWriteOffDetails--�ʲ������ڳ���������
(
	SrinWriteOffDetailsID int identity(1,1) primary key not null,--����
	SrinQualifiedReceiptID int not null foreign key references SrinQualifiedReceipt(SrinQualifiedReceiptID),--���ռ��鴫�ݱ�
	StorageOutRealDetailsID int not null foreign key references StorageOutRealDetails(StorageOutRealDetailsID),--��Ӧ�ĳ�������
	SrinAssetQualifiedDetailsID int not null foreign key references SrinAssetQualifiedDetails(SrinAssetQualifiedDetailsID),--��Ӧ���ʼ�ϸ�����
	Gentaojian decimal(18,2) not null,--�ϸ�����
	Metre decimal(18,2) not null,--������
	Ton decimal(18,2) not null,--������
	Amount decimal(18,2) not null,--���
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID)--������
)

go
create table SrinRepairReport--��������޸������
(
	SrinRepairReportID int identity(1,1) primary key not null,--����
	SrinInspectorVerifyTransferID int not null foreign key references SrinInspectorVerifyTransfer(SrinInspectorVerifyTransferID),--�����Ļ��ռ��鴫�ݱ�
	SrinRepairReportCode nvarchar(50) not null unique,--�����	
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	TaskID int not null foreign key references TaskStorageIn(TaskStorageID)--����������
)

go

create table SrinInspectorVerifyRDetails--�����޸�����--�ʼ���
(
	SrinInspectorVerifyRDetailsID int identity(1,1) primary key not null,--����
	SrinInspectorVerifyDetailsID int not null foreign key references SrinInspectorVerifyDetails(SrinInspectorVerifyDetailsID),--��Ӧ���ʼ�����
	QualifiedGentaojian decimal(18,2) not null,--�ϸ�����	
	RejectGentaojian decimal(18,2) not null,--����������
	VerifyCode nvarchar(50),--�ʼ챨���
	VerifyTime datetime not null,--�ʼ�ʱ��
	Remark nvarchar(200),
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID)--������
)

go

--*********************************************************
--��������������ݱ�
--Created By: adonis
--Date:2010.8.17-2010.8.21
--*********************************************************
create table AwaitScrap --�����ϱ�
(
	AwaitScrapID int identity(1,1) primary key not null, --����
	ScrapReportNum nvarchar(50) not null default('δ��д'),--�������ʱ����(��������д��˱�ʱ����д���ֶ�)
	State nvarchar(10) not null check(State in('������','�ѱ���')),--����״̬
	
	MaterialID int not null foreign key references MaterialInfo(MaterialID),--������Ϣ���������ơ�����ͺš��������
	ManufactureID int not null foreign key references Manufacturer(ManufacturerID),--��������,���
	Gentaojian decimal(18,2) not null,--��������
	StorageID int not null foreign key references StorageInfo(StorageID),--���ڲֿ�
	PileID int not null foreign key references PileInfo(PileID),--������λ
	ProjectID int foreign key references ProjectInfo(ProjectID),--������ĿID
	TransferID int, --����SrinInspectorVerifyTransfer�����ռ��鴫�ݱ�ID
	TransferType nvarchar(10) not null check(TransferType in ('�޸�����','��������')),--��ʶ�������޸���ı��ϻ���ֱ���ʼ��ı���
	Creator int not null foreign key references EmpInfo(EmpID),--������
	CreateTime datetime not null,--����ʱ��
	Remark nvarchar(200)
)
go



create table Scrapped   --���ϱ�
(
	ScrappedID int identity(1,1) primary key not null, --����
	AwaitScrapID int not null foreign key references AwaitScrap(AwaitScrapID),--�����ϱ����
	ScrappedNum nvarchar(50) not null,--�����ļ���
	StockID int,--�����ܿ���ID
	StockType nvarchar(50),--�����ܿ������(���£����ϣ�����)
	ScrappedTime datetime not null,--����ʱ��
	CreateTime datetime not null,--����ʱ��
	Creator int not null foreign key references EmpInfo(EmpID),--������
	Remark nvarchar(200)
)
go


--*********************************************************
--Ԥ����
--*********************************************************

create table WarningList   --���ϱ�
(
	WarningID int identity(1,1) primary key not null, --����
	MaterialID int not null unique foreign key references MaterialInfo(MaterialID),--���ϱ���
	QuantityGentaojian decimal(18,2),--��/̨/��/������
    QuantityMetre decimal(18,2), --�׵�����
    QuantityTon decimal(18,2),--�ֵ�����
)
go

--*********************************************************
--�ʼ챨���
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
--��ͼ��
--*********************************************************
        


create view StorageStocks--���ʿ����ͼ
as
SELECT     dbo.TableOfStocks.BatchIndex, dbo.MaterialInfo.MaterialName, dbo.MaterialInfo.SpecificationModel, dbo.MaterialInfo.FinanceCode, 
                      dbo.TableOfStocks.MaterialCode, dbo.PileInfo.PileName, dbo.StorageInfo.StorageName, dbo.TableOfStocks.StocksID, dbo.TableOfStocks.UnitPrice, 
                      dbo.TableOfStocks.StorageTime, dbo.TableOfStocks.Remark, '����' AS Status, dbo.TableOfStocks.MaterialID, dbo.StorageInfo.StorageID, 
                      dbo.TableOfStocks.PileID, dbo.TableOfStocks.QuantityGentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_2
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) AS StocksGenTaojian, dbo.TableOfStocks.QuantityMetre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_5
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) AS StocksMetre, dbo.TableOfStocks.QuantityTon -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) AS StocksTon, dbo.TableOfStocks.CurUnit, 
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
                      dbo.StockOnline.StorageTime, dbo.StockOnline.Remark, '����' AS Status, dbo.StockOnline.MaterialID, StorageInfo_1.StorageID, dbo.StockOnline.PileID, 
                      dbo.StockOnline.QuantityGentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_2
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) AS StocksGenTaojian, dbo.StockOnline.QuantityMetre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_5
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) AS StocksMetre, dbo.StockOnline.QuantityTon -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) AS StocksTon, dbo.StockOnline.CurUnit, 
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
                      dbo.QualifiedStocks.Remark, '���պϸ�' AS Status, dbo.QualifiedStocks.MaterialID, dbo.QualifiedStocks.StorageID, dbo.QualifiedStocks.PileID, 
                      dbo.QualifiedStocks.Gentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_3
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) AS StocksGenTaojian, dbo.QualifiedStocks.Metre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_2
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_3
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) AS StocksMetre, dbo.QualifiedStocks.Ton -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) AS StocksTon, dbo.QualifiedStocks.CurUnit, 
                      Manufacturer_2.ManufacturerName, dbo.QualifiedStocks.ManufactureID
FROM         dbo.QualifiedStocks INNER JOIN
                      dbo.Manufacturer AS Manufacturer_2 ON dbo.QualifiedStocks.ManufactureID = Manufacturer_2.ManufacturerID INNER JOIN
                      dbo.ProjectInfo AS ProjectInfo_1 ON dbo.QualifiedStocks.RetrieveProjectID = ProjectInfo_1.ProjectID INNER JOIN
                      dbo.MaterialInfo AS MaterialInfo_2 ON dbo.QualifiedStocks.MaterialID = MaterialInfo_2.MaterialID INNER JOIN
                      dbo.StorageInfo AS StorageInfo_2 ON dbo.QualifiedStocks.StorageID = StorageInfo_2.StorageID INNER JOIN
                      dbo.PileInfo AS PileInfo_2 ON dbo.QualifiedStocks.StorageID = PileInfo_2.PileID
go         
create view WriteOffDetails--������ͼ
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
   
 create view     NormalIn    ----�����������,��������ѯ��
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
WHERE     (dbo.StorageDirector.Approve = N'��')
go


create view WaitForTest--����
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
                            FROM          dbo.StorageInTest))) AND (dbo.StorageInMaterialsLeader.Auditing = N'��')
go

create view NormalOut--����
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
                      
create view ViewCommitIn--ί����
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
WHERE     (dbo.CommitDirector.Approve = N'��')
                      
go
create view ReportStocks--��������ͼ
as              
                      
     SELECT     dbo.TableOfStocks.BatchIndex, dbo.MaterialInfo.MaterialName, dbo.MaterialInfo.SpecificationModel, dbo.MaterialInfo.FinanceCode, 
                      dbo.TableOfStocks.BillCode, dbo.SupplierInfo.SupplierName, dbo.PileInfo.PileCode, dbo.TableOfStocks.StorageInCode, 
                      dbo.TableOfStocks.MaterialCode, dbo.PileInfo.PileName, dbo.StorageInfo.StorageName, dbo.TableOfStocks.StocksID, dbo.TableOfStocks.UnitPrice, 
                      dbo.TableOfStocks.StorageTime, dbo.TableOfStocks.Remark, '����' AS Status, dbo.TableOfStocks.MaterialID, dbo.StorageInfo.StorageID, 
                      dbo.TableOfStocks.PileID, dbo.TableOfStocks.QuantityGentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_2
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) AS StocksGenTaojian, dbo.TableOfStocks.QuantityMetre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_5
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) AS StocksMetre, dbo.TableOfStocks.QuantityTon -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.TableOfStocks.StocksID = StocksID) AND (MaterialStatus = '����')) AS StocksTon, dbo.TableOfStocks.CurUnit, 
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
                      dbo.StockOnline.Remark, '����' AS Status, dbo.StockOnline.MaterialID, StorageInfo_1.StorageID, dbo.StockOnline.PileID, 
                      dbo.StockOnline.QuantityGentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_2
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) AS StocksGenTaojian, dbo.StockOnline.QuantityMetre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_5
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) AS StocksMetre, dbo.StockOnline.QuantityTon -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.StockOnline.StockOnlineID = StocksID) AND (MaterialStatus = '����')) AS StocksTon, dbo.StockOnline.CurUnit, 
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
                      dbo.QualifiedStocks.StocksID, dbo.QualifiedStocks.UnitPrice, dbo.QualifiedStocks.StorageTime, dbo.QualifiedStocks.Remark, '���պϸ�' AS Status, 
                      dbo.QualifiedStocks.MaterialID, dbo.QualifiedStocks.StorageID, dbo.QualifiedStocks.PileID, dbo.QualifiedStocks.Gentaojian -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS OutGentaojian
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_3
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) -
                          (SELECT     ISNULL(SUM(RealGentaojian), 0) AS CommitOutGentaojian
                            FROM          dbo.StorageCommitOutRealDetails
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) AS StocksGenTaojian, dbo.QualifiedStocks.Metre -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS OutMetre
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_2
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) -
                          (SELECT     ISNULL(SUM(RealMetre), 0) AS CommitOutMetre
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_3
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) AS StocksMetre, dbo.QualifiedStocks.Ton -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS OutTon
                            FROM          dbo.StorageOutRealDetails AS StorageOutRealDetails_4
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) -
                          (SELECT     ISNULL(SUM(RealTon), 0) AS CommitOutTon
                            FROM          dbo.StorageCommitOutRealDetails AS StorageCommitOutRealDetails_1
                            WHERE      (dbo.QualifiedStocks.StocksID = StocksID) AND (MaterialStatus = '���պϸ�')) AS StocksTon, dbo.QualifiedStocks.CurUnit, 
                      Manufacturer_2.ManufacturerName, dbo.QualifiedStocks.ManufactureID
FROM         dbo.QualifiedStocks INNER JOIN
                      dbo.Manufacturer AS Manufacturer_2 ON dbo.QualifiedStocks.ManufactureID = Manufacturer_2.ManufacturerID INNER JOIN
                      dbo.ProjectInfo AS ProjectInfo_1 ON dbo.QualifiedStocks.RetrieveProjectID = ProjectInfo_1.ProjectID INNER JOIN
                      dbo.MaterialInfo AS MaterialInfo_2 ON dbo.QualifiedStocks.MaterialID = MaterialInfo_2.MaterialID INNER JOIN
                      dbo.StorageInfo AS StorageInfo_2 ON dbo.QualifiedStocks.StorageID = StorageInfo_2.StorageID INNER JOIN
                      dbo.PileInfo AS PileInfo_2 ON dbo.QualifiedStocks.StorageID = PileInfo_2.PileID
go                 
create view FlowDetails--ת������������ͼ
as 

SELECT     dbo.StorageCommitOutNotice.StorageCommitOutNoticeCode, dbo.ProjectInfo.ProjectName, dbo.TableOfStocks.CurUnit, 
                      dbo.StorageCommitOutRealDetails.RealGentaojian, dbo.StorageCommitOutRealDetails.RealMetre, dbo.StorageCommitOutRealDetails.RealTon, 
                      'ί�����' AS type, dbo.TableOfStocks.StocksID, dbo.ProjectInfo.ProjectID
FROM         dbo.ProjectInfo INNER JOIN
                      dbo.TableOfStocks ON dbo.ProjectInfo.ProjectID = dbo.TableOfStocks.ExpectedProject INNER JOIN
                      dbo.StorageCommitOutRealDetails ON dbo.TableOfStocks.StocksID = dbo.StorageCommitOutRealDetails.StocksID INNER JOIN
                      dbo.StorageCommitOutNotice ON 
                      dbo.StorageCommitOutRealDetails.StorageCommitOutNoticeID = dbo.StorageCommitOutNotice.StorageCommitOutNoticeID
UNION ALL
SELECT     dbo.StorageOutNotice.StorageOutNoticeCode, ProjectInfo_1.ProjectName, TableOfStocks_1.CurUnit, dbo.StorageOutRealDetails.RealGentaojian, 
                      dbo.StorageOutRealDetails.RealMetre, dbo.StorageOutRealDetails.RealTon, '��������' AS type, TableOfStocks_1.StocksID, 
                      ProjectInfo_1.ProjectID
FROM         dbo.TableOfStocks AS TableOfStocks_1 INNER JOIN
                      dbo.StorageOutRealDetails ON TableOfStocks_1.StocksID = dbo.StorageOutRealDetails.StocksID INNER JOIN
                      dbo.ProjectInfo AS ProjectInfo_1 ON TableOfStocks_1.ExpectedProject = ProjectInfo_1.ProjectID INNER JOIN
                      dbo.StorageOutNotice ON dbo.StorageOutRealDetails.StorageOutNoticeID = dbo.StorageOutNotice.StorageOutNoticeID
                      
                      
                      
                     
                     