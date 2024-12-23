/****** Replace with ur db name ******/
USE [MachineDB]
GO
/****** Object:  Table [dbo].[ErrorCodeCategories]    Script Date: 2024/12/8 下午 05:39:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ErrorCodeCategories](
	[ID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DataType] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ErrorCodeMappings]    Script Date: 2024/12/8 下午 05:39:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ErrorCodeMappings](
	[ID] [uniqueidentifier] NOT NULL,
	[CategoryID] [uniqueidentifier] NOT NULL,
	[ConditionString] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](50) NULL,
 CONSTRAINT [PK_ErrorCodeMappings] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LogicStatusCategories]    Script Date: 2024/12/8 下午 05:39:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogicStatusCategories](
	[ID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DataType] [int] NOT NULL,
 CONSTRAINT [PK_LogicStatusCategories] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LogicStatusConditions]    Script Date: 2024/12/8 下午 05:39:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogicStatusConditions](
	[Id] [uniqueidentifier] NOT NULL,
	[CategoryID] [uniqueidentifier] NOT NULL,
	[CondictionString] [nvarchar](50) NOT NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_LogicStatusCondictions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Machine]    Script Date: 2024/12/8 下午 05:39:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Machine](
	[ID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[IP] [nvarchar](50) NOT NULL,
	[Port] [int] NOT NULL,
	[ConnectionType] [int] NOT NULL,
	[MaxRetryCount] [int] NOT NULL,
	[TagCategoryID] [uniqueidentifier] NULL,
	[LogicStatusCategoryID] [uniqueidentifier] NULL,
	[ErrorCodeCategoryID] [uniqueidentifier] NULL,
	[Enabled] [bit] NOT NULL,
	[UpdateDelay] [int] NOT NULL,
 CONSTRAINT [PK__Machine__3214EC274421D01E] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ__Machine__737584F66D3A72BE] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tag]    Script Date: 2024/12/8 下午 05:39:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tag](
	[ID] [uniqueidentifier] NOT NULL,
	[CategoryID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DataType] [int] NOT NULL,
	[UpdateByTime] [bit] NOT NULL,
	[SpecialType] [int] NOT NULL,
	[Bool_1] [bit] NOT NULL,
	[Bool_2] [bit] NOT NULL,
	[Bool_3] [bit] NOT NULL,
	[Bool_4] [bit] NOT NULL,
	[Bool_5] [bit] NOT NULL,
	[Int_1] [int] NOT NULL,
	[Int_2] [int] NOT NULL,
	[Int_3] [int] NOT NULL,
	[Int_4] [int] NOT NULL,
	[Int_5] [int] NOT NULL,
	[String_1] [nvarchar](50) NULL,
	[String_2] [nvarchar](50) NULL,
	[String_3] [nvarchar](50) NULL,
	[String_4] [nvarchar](50) NULL,
	[String_5] [nvarchar](50) NULL,
 CONSTRAINT [PK__Tag__3214EC271E77490D] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ__Tag__737584F665FA3403] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TagCategory]    Script Date: 2024/12/8 下午 05:39:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TagCategory](
	[ID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[ConnectionType] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Tag]  WITH CHECK ADD  CONSTRAINT [FK__Tag__CategoryID__68487DD7] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[TagCategory] ([ID])
GO
ALTER TABLE [dbo].[Tag] CHECK CONSTRAINT [FK__Tag__CategoryID__68487DD7]
GO
