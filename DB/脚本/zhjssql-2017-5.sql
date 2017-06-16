/*==============================================================*/
/* DBMS name:      Microsoft SQL Server 2008                    */
/* Created on:     2017/3/29 1:00:25                            */
/*==============================================================*/


if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Collection') and o.name = 'FK_Z_COLLEC_RELATIONS_Z_EQUIPM')
alter table Z_Collection
   drop constraint FK_Z_COLLEC_RELATIONS_Z_EQUIPM
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Course') and o.name = 'FK_Z_COURSE_RELATIONS_Z_SECTIO')
alter table Z_Course
   drop constraint FK_Z_COURSE_RELATIONS_Z_SECTIO
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Course') and o.name = 'FK_Z_COURSE_RELATIONS_Z_ROOM')
alter table Z_Course
   drop constraint FK_Z_COURSE_RELATIONS_Z_ROOM
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Equipment') and o.name = 'FK_Z_EQUIPM_RELATIONS_Z_ROOM')
alter table Z_Equipment
   drop constraint FK_Z_EQUIPM_RELATIONS_Z_ROOM
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Equipment') and o.name = 'FK_Z_EQU_T_RELATIONS_Z_EQUIPM')
alter table Z_Equipment
   drop constraint FK_Z_EQU_T_RELATIONS_Z_EQUIPM
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_EquipmentLog') and o.name = 'FK_Z_EQU_RELATIONS_Z_EQULOG')
alter table Z_EquipmentLog
   drop constraint FK_Z_EQU_RELATIONS_Z_EQULOG
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_EquipmentLog') and o.name = 'FK_Z_EQUIPM_RELATIONS_Z_LOGTYP')
alter table Z_EquipmentLog
   drop constraint FK_Z_EQUIPM_RELATIONS_Z_LOGTYP
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Floor') and o.name = 'FK_Z_FLOOR_RELATIONS_Z_BUILDI')
alter table Z_Floor
   drop constraint FK_Z_FLOOR_RELATIONS_Z_BUILDI
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Parameter') and o.name = 'FK_Z_PARAME_RELATIONS_Z_EQUIPM')
alter table Z_Parameter
   drop constraint FK_Z_PARAME_RELATIONS_Z_EQUIPM
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Room') and o.name = 'FK_Z_ROOM_RELATIONS_Z_FLOOR')
alter table Z_Room
   drop constraint FK_Z_ROOM_RELATIONS_Z_FLOOR
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('Z_Room') and o.name = 'FK_Z_ROOM_RELATIONS_Z_ROOMTY')
alter table Z_Room
   drop constraint FK_Z_ROOM_RELATIONS_Z_ROOMTY
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Building')
            and   type = 'U')
   drop table Z_Building
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Collection')
            and   type = 'U')
   drop table Z_Collection
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Course')
            and   type = 'U')
   drop table Z_Course
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Equipment')
            and   type = 'U')
   drop table Z_Equipment
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_EquipmentLog')
            and   type = 'U')
   drop table Z_EquipmentLog
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_EquipmentType')
            and   type = 'U')
   drop table Z_EquipmentType
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Floor')
            and   type = 'U')
   drop table Z_Floor
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_LogType')
            and   type = 'U')
   drop table Z_LogType
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Parameter')
            and   type = 'U')
   drop table Z_Parameter
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Room')
            and   type = 'U')
   drop table Z_Room
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_RoomType')
            and   type = 'U')
   drop table Z_RoomType
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Section')
            and   type = 'U')
   drop table Z_Section
go

if exists (select 1
            from  sysobjects
           where  id = object_id('Z_Teacher')
            and   type = 'U')
   drop table Z_Teacher
go

if exists(select 1 from systypes where name='bool')
   drop type bool
go

if exists(select 1 from systypes where name='datetime')
   drop type datetime
go

if exists(select 1 from systypes where name='int')
   drop type int
go

if exists(select 1 from systypes where name='varchar(20)')
   drop type "varchar(20)"
go

if exists(select 1 from systypes where name='varchar(50)')
   drop type "varchar(50)"
go

if exists(select 1 from systypes where name='varchar(500)')
   drop type "varchar(500)"
go

/*==============================================================*/
/* Domain: bool                                                 */
/*==============================================================*/
create type bool
   from bit
go

/*==============================================================*/
/* Domain: datetime                                             */
/*==============================================================*/
create type datetime
   from datetime
go

/*==============================================================*/
/* Domain: int                                                  */
/*==============================================================*/
create type int
   from int
go

/*==============================================================*/
/* Domain: "varchar(20)"                                        */
/*==============================================================*/
create type "varchar(20)"
   from varchar(20)
go

/*==============================================================*/
/* Domain: "varchar(50)"                                        */
/*==============================================================*/
create type "varchar(50)"
   from varchar(50)
go

/*==============================================================*/
/* Domain: "varchar(500)"                                       */
/*==============================================================*/
create type "varchar(500)"
   from varchar(500)
go

/*==============================================================*/
/* Table: Z_Building                                            */
/*==============================================================*/
create table Z_Building (
   F_Id                 "varchar(50)"        not null,
   F_FullName           "varchar(50)"        null,
   F_EnCode             "varchar(50)"        null,
   F_ShortName          "varchar(50)"        null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_BUILDING primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '楼栋信息表',
   'user', @CurrentUser, 'table', 'Z_Building'
go

/*==============================================================*/
/* Table: Z_Collection                                          */
/*==============================================================*/
create table Z_Collection (
   F_Id                 varchar(50)          not null,
   F_EquipmentId        "varchar(50)"        not null,
   F_Status             bool                 null,
   F_FaultStatus        bool                 null,
   F_Value1             varchar(50)          null,
   F_Value2             varchar(50)          null,
   F_Value3             varchar(50)          null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_COLLECTION primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '采集信息表',
   'user', @CurrentUser, 'table', 'Z_Collection'
go

/*==============================================================*/
/* Table: Z_Course                                              */
/*==============================================================*/
create table Z_Course (
   F_Id                 "varchar(50)"        not null,
   F_RoomId             "varchar(50)"        not null,
   F_SectionId          "varchar(50)"        not null,
   F_FullName           "varchar(50)"        null,
   F_TeacherId          varchar(50)          null,
   F_TeacherName        varchar(50)          null,
   F_Major              varchar(100)         null,
   F_Grade              varchar(50)          null,
   F_Class              varchar(100)         null,
   F_Students           int                  null,
   F_SchoolYear         varchar(50)          null,
   F_Term               varchar(50)          null,
   F_BeginWeek          varchar(50)          null,
   F_EndWeek            varchar(50)          null,
   F_WeekCount          varchar(50)          null,
   F_Week               varchar(50)          null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_COURSE primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '课程信息表',
   'user', @CurrentUser, 'table', 'Z_Course'
go

/*==============================================================*/
/* Table: Z_Equipment                                           */
/*==============================================================*/
create table Z_Equipment (
   F_Id                 "varchar(50)"        not null,
   F_RoomId             "varchar(50)"        not null,
   F_EquipmentTypeId    varchar(50)          not null,
   F_FullName           "varchar(50)"        null,
   F_Code               varchar(50)          null,
   F_Model              varchar(100)         null,
   F_Specifications     varchar(100)         null,
   F_Brand              varchar(50)          null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_EQUIPMENT primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '设备信息表',
   'user', @CurrentUser, 'table', 'Z_Equipment'
go

/*==============================================================*/
/* Table: Z_EquipmentLog                                        */
/*==============================================================*/
create table Z_EquipmentLog (
   F_Id                 varchar(50)          not null,
   F_EquipmentId        varchar(50)          null,
   F_LogTypeId          varchar(50)          not null,
   F_FullName           varchar(100)         null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_EQUIPMENTLOG primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '设备日志表',
   'user', @CurrentUser, 'table', 'Z_EquipmentLog'
go

/*==============================================================*/
/* Table: Z_EquipmentType                                       */
/*==============================================================*/
create table Z_EquipmentType (
   F_Id                 varchar(50)          not null,
   F_FullName           "varchar(50)"        null,
   F_EnCode             "varchar(50)"        null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_EQUIPMENTTYPE primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '设备类型表',
   'user', @CurrentUser, 'table', 'Z_EquipmentType'
go

/*==============================================================*/
/* Table: Z_Floor                                               */
/*==============================================================*/
create table Z_Floor (
   F_Id                 varchar(50)          not null,
   F_BuildingId         varchar(50)          not null,
   F_FullName           "varchar(50)"        null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_FLOOR primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '楼层信息表',
   'user', @CurrentUser, 'table', 'Z_Floor'
go

/*==============================================================*/
/* Table: Z_LogType                                             */
/*==============================================================*/
create table Z_LogType (
   F_Id                 "varchar(50)"        not null,
   F_FullName           varchar(100)         null,
   F_Level              int                  null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_LOGTYPE primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '日志类型信息表',
   'user', @CurrentUser, 'table', 'Z_LogType'
go

/*==============================================================*/
/* Table: Z_Parameter                                           */
/*==============================================================*/
create table Z_Parameter (
   F_Id                 "varchar(50)"        not null,
   F_EquipmentTypeId    "varchar(50)"        not null,
   F_FullName           "varchar(50)"        null,
   F_Open               bit                  null,
   F_Fault              bit                  null,
   F_ParName            varchar(50)          null,
   F_Begin              varchar(50)          null,
   F_End                varchar(50)          null,
   F_Unit               varchar(50)          null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_PARAMETER primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '设备参数表',
   'user', @CurrentUser, 'table', 'Z_Parameter'
go

/*==============================================================*/
/* Table: Z_Room                                                */
/*==============================================================*/
create table Z_Room (
   F_Id                 "varchar(50)"        not null,
   F_FloorId            "varchar(50)"        not null,
   F_RoomTypeId         "varchar(50)"        not null,
   F_FullName           "varchar(50)"        null,
   F_Number             varchar(50)          null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_ROOM primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '房间信息表',
   'user', @CurrentUser, 'table', 'Z_Room'
go

/*==============================================================*/
/* Table: Z_RoomType                                            */
/*==============================================================*/
create table Z_RoomType (
   F_Id                 "varchar(50)"        not null,
   F_FullName           "varchar(50)"        null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_ROOMTYPE primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '房间类型表',
   'user', @CurrentUser, 'table', 'Z_RoomType'
go

/*==============================================================*/
/* Table: Z_Section                                             */
/*==============================================================*/
create table Z_Section (
   F_Id                 varchar(50)          not null,
   F_SectionName        varchar(50)          null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_SECTION primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '节数类型信息表',
   'user', @CurrentUser, 'table', 'Z_Section'
go

/*==============================================================*/
/* Table: Z_Teacher                                             */
/*==============================================================*/
create table Z_Teacher (
   F_Id                 varchar(50)          not null,
   F_TeacherName        varchar(50)          null,
   F_SortCode           int                  null,
   F_DeleteMark         bool                 null,
   F_EnabledMark        bool                 null,
   F_Description        "varchar(500)"       null,
   F_CreatorTime        datetime             null,
   F_CreatorUserId      "varchar(50)"        null,
   F_LastModifyTime     datetime             null,
   F_LastModifyUserId   "varchar(50)"        null,
   F_DeleteTime         datetime             null,
   F_DeleteUserId       "varchar(500)"       null,
   constraint PK_Z_TEACHER primary key nonclustered (F_Id)
)
go

declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sys.sp_addextendedproperty 'MS_Description', 
   '教师信息表',
   'user', @CurrentUser, 'table', 'Z_Teacher'
go

alter table Z_Collection
   add constraint FK_Z_COLLEC_RELATIONS_Z_EQUIPM foreign key (F_EquipmentId)
      references Z_Equipment (F_Id)
go

alter table Z_Course
   add constraint FK_Z_COURSE_RELATIONS_Z_SECTIO foreign key (F_SectionId)
      references Z_Section (F_Id)
go

alter table Z_Course
   add constraint FK_Z_COURSE_RELATIONS_Z_ROOM foreign key (F_RoomId)
      references Z_Room (F_Id)
go

alter table Z_Equipment
   add constraint FK_Z_EQUIPM_RELATIONS_Z_ROOM foreign key (F_RoomId)
      references Z_Room (F_Id)
go

alter table Z_Equipment
   add constraint FK_Z_EQU_T_RELATIONS_Z_EQUIPM foreign key (F_EquipmentTypeId)
      references Z_EquipmentType (F_Id)
go

alter table Z_EquipmentLog
   add constraint FK_Z_EQU_RELATIONS_Z_EQULOG foreign key (F_EquipmentId)
      references Z_Equipment (F_Id)
go

alter table Z_EquipmentLog
   add constraint FK_Z_EQUIPM_RELATIONS_Z_LOGTYP foreign key (F_Id)
      references Z_LogType (F_Id)
go

alter table Z_Floor
   add constraint FK_Z_FLOOR_RELATIONS_Z_BUILDI foreign key (F_BuildingId)
      references Z_Building (F_Id)
go

alter table Z_Parameter
   add constraint FK_Z_PARAME_RELATIONS_Z_EQUIPM foreign key (F_EquipmentTypeId)
      references Z_EquipmentType (F_Id)
go

alter table Z_Room
   add constraint FK_Z_ROOM_RELATIONS_Z_FLOOR foreign key (F_FloorId)
      references Z_Floor (F_Id)
go

alter table Z_Room
   add constraint FK_Z_ROOM_RELATIONS_Z_ROOMTY foreign key (F_RoomTypeId)
      references Z_RoomType (F_Id)
go

