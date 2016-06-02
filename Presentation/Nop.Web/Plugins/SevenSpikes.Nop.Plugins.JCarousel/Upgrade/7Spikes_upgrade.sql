-- 7Spikes upgrade scripts from nopCommerce 3.60 to 3.70


-- Create the [SS_ES_EntitySetting] table if not exists
BEGIN TRANSACTION;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN

	IF(NOT EXISTS (SELECT NULL FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SS_ES_EntitySetting]')))
	BEGIN
		SET ANSI_NULLS ON;

		SET QUOTED_IDENTIFIER ON;

		CREATE TABLE [dbo].[SS_ES_EntitySetting](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[EntityType] [int] NOT NULL,
			[EntityId] [int] NOT NULL,
			[Key] [nvarchar](max) NULL,
			[Value] [nvarchar](max) NULL,
		PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	END

END
COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO

-- Function for converting BIT field value (0|1) to string bool value (True|False)
IF OBJECT_ID ( N'dbo.ConvertToStringBool', N'FN' ) IS NOT NULL 
	DROP FUNCTION dbo.ConvertToStringBool;
GO

CREATE FUNCTION dbo.ConvertToStringBool (@BitValue BIT)
RETURNS nvarchar(MAX)
AS
BEGIN
	IF (@BitValue = 1)
		RETURN 'True';
	
	RETURN 'False';
END
GO


-- SALES CAMPAIGNS plugin
BEGIN TRANSACTION;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

-- Check if the Sales Campaign plugin is installed
IF(EXISTS (SELECT NULL FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SS_SC_SaleCampaignCustomization]') AND type in (N'U')))
BEGIN
	-- fetch rows from the SS_SC_SaleCampaignCustomization table
	DECLARE @campaignCursor CURSOR

	-- old table - declare the needed variables
	DECLARE @CampaignId INT
	DECLARE @ClockType INT
	DECLARE @ClockTextAlign INT
	DECLARE @FontWeight INT
	DECLARE @FontStyle INT
	DECLARE @FontSize INT
	DECLARE @FontColor nvarchar(MAX)
	DECLARE @BackgroundColor nvarchar(MAX)
	DECLARE @MarginSize nvarchar(MAX)
	DECLARE @PaddingSize nvarchar(MAX)
	DECLARE @CustomCSS nvarchar(MAX)
	
	-- new table - declare the needed variables
	DECLARE @EntityTypeCampaign INT
	DECLARE @NewClockType nvarchar(MAX)
	DECLARE @NewClockTextAlign nvarchar(MAX)
	DECLARE @NewFontWeight nvarchar(MAX)
	DECLARE @NewFontStyle nvarchar(MAX)
	DECLARE @NewFontSize nvarchar(MAX)
	DECLARE @NewFontColor nvarchar(MAX)
	DECLARE @NewBackgroundColor nvarchar(MAX)
	DECLARE @MarginTop nvarchar(MAX)
	DECLARE @MarginRight nvarchar(MAX)
	DECLARE @MarginBottom nvarchar(MAX)
	DECLARE @MarginLeft nvarchar(MAX)
	DECLARE @PaddingTop nvarchar(MAX)
	DECLARE @PaddingRight nvarchar(MAX)
	DECLARE @PaddingBottom nvarchar(MAX)
	DECLARE @PaddingLeft nvarchar(MAX)
	DECLARE @NewCustomCSS nvarchar(MAX)
	
	DECLARE @start INT;
	DECLARE @end INT;
	
	-- EntityType
	SET @EntityTypeCampaign = 40;

	-- set the cursor
	SET @campaignCursor = CURSOR FOR
	SELECT [Id],[ClockType],[ClockTextAlign],[FontWeight],[FontStyle],[FontSize],[FontColor],[BackgroundColor],[MarginSize],[PaddingSize],[CustomCSS]
	FROM [dbo].[SS_SC_SaleCampaignCustomization]
	
	-- open the cursor
	OPEN @campaignCursor
		
	-- initial fetch, so we can enter the while loop
	FETCH NEXT
	FROM @campaignCursor 
	INTO @CampaignId, @ClockType, @ClockTextAlign, @FontWeight, @FontStyle, @FontSize, @FontColor, @BackgroundColor, @MarginSize, @PaddingSize, @CustomCSS

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP
	
		-- ClockType
		SET @NewClockType = 
		(
		CASE
			--WHEN @clockType = 5 THEN 'DaysHoursMinutesSeconds'-- commented because the else handles it / is the same
			WHEN @ClockType = 10 THEN 'HoursMinutesSeconds'
			WHEN @ClockType = 15 THEN 'OnlyDays'
			WHEN @ClockType = 20 THEN 'LongDate'
			WHEN @ClockType = 25 THEN 'LongDateWithoutTime'
			WHEN @ClockType = 30 THEN 'ShortDate'
			WHEN @ClockType = 35 THEN 'ShortDateWithoutTime'
			ELSE 'DaysHoursMinutesSeconds'
		END
		);
		
		-- ClockTextAlign
		SET @NewClockTextAlign = 
		(
		CASE
			--WHEN @ClockTextAlign = 5 THEN 'Left'-- commented because the else handles it / is the same
			WHEN @ClockTextAlign = 10 THEN 'Right'
			WHEN @ClockTextAlign = 15 THEN 'Center'
			WHEN @ClockTextAlign = 20 THEN 'Justify'
			WHEN @ClockTextAlign = 25 THEN 'Inherit'
			ELSE 'Left'
		END
		);
		
		-- FontWeight
		SET @NewFontWeight = 
		(
		CASE
			--WHEN @FontWeight = 5 THEN 'Normal'-- commented because the else handles it / is the same
			WHEN @FontWeight = 10 THEN 'Bold'
			WHEN @FontWeight = 15 THEN 'Bolder'
			WHEN @FontWeight = 20 THEN 'Lighter'
			ELSE 'Normal'
		END
		);
		
		-- FontStyle
		SET @NewFontStyle = 
		(
		CASE
			--WHEN @FontStyle = 5 THEN 'Normal'-- commented because the else handles it / is the same
			WHEN @FontStyle = 10 THEN 'Italic'
			WHEN @FontStyle = 15 THEN 'Oblique'
			ELSE 'Normal'
		END
		);
		
		-- FontSize
		SET @NewFontSize = CAST(@FontSize AS nvarchar(MAX));
		
		-- FontColor
		SET @NewFontColor = @FontColor;
		
		-- BackgroundColor
		SET @NewBackgroundColor = @BackgroundColor;
		
		-- MarginSize
		IF (LEN(@MarginSize) > 6)-- we must have at least 7 symbols, e.g. 3,3,3,3
		BEGIN
			SET @start = 0;
			SET @end = CHARINDEX(',', @MarginSize, @start);		
			SET @MarginTop = SUBSTRING(@MarginSize, 0, @end);

			SET @start = @end+1;
			SET @end = CHARINDEX(',', @MarginSize, @start);
			SET @MarginRight = SUBSTRING(@MarginSize, @start, @end - @start);

			SET @start = @end+1;
			SET @end = CHARINDEX(',', @MarginSize, @start);
			SET @MarginBottom = SUBSTRING(@MarginSize, @start, @end - @start);

			SET @start = @end+1;
			SET @end = CHARINDEX(',', @MarginSize, @start);
			SET @MarginLeft = SUBSTRING(@MarginSize, @start, LEN(@MarginSize));
		END
		
		-- PaddingSize
		IF (LEN(@PaddingSize) > 6)-- we must have at least 7 symbols, e.g. 3,3,3,3
		BEGIN
			SET @start = 0;
			SET @end = CHARINDEX(',', @PaddingSize, @start);		
			SET @PaddingTop = SUBSTRING(@PaddingSize, 0, @end);

			SET @start = @end+1;
			SET @end = CHARINDEX(',', @PaddingSize, @start);
			SET @PaddingRight = SUBSTRING(@PaddingSize, @start, @end - @start);

			SET @start = @end+1;
			SET @end = CHARINDEX(',', @PaddingSize, @start);
			SET @PaddingBottom = SUBSTRING(@PaddingSize, @start, @end - @start);

			SET @start = @end+1;
			SET @end = CHARINDEX(',', @PaddingSize, @start);
			SET @PaddingLeft = SUBSTRING(@PaddingSize, @start, LEN(@PaddingSize));
		END
		
		-- CustomCSS
		SET @NewCustomCSS = @CustomCSS;

		

		-- Insert into entity settings
		INSERT INTO [dbo].[SS_ES_EntitySetting]
		([EntityType],[EntityId],[Key],[Value])
		VALUES
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.clocktype', @NewClockType),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.clocktextalign', @NewClockTextAlign),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.fontweight', @NewFontWeight),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.fontstyle', @NewFontStyle),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.fontsize', @NewFontSize),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.fontcolor', @NewFontColor),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.backgroundcolor', @NewBackgroundColor),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.margintop', @MarginTop),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.marginright', @MarginRight),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.marginbottom', @MarginBottom),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.marginleft', @MarginLeft),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.paddingtop', @PaddingTop),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.paddingright', @PaddingRight),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.paddingbottom', @PaddingBottom),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.paddingleft', @PaddingLeft),
		(@EntityTypeCampaign, @CampaignId, 'salecampaignproductpagecustomization.customcss', @NewCustomCSS)
		


		-- fetch the next row
		FETCH NEXT
		FROM @campaignCursor 
		INTO @CampaignId, @ClockType, @ClockTextAlign, @FontWeight, @FontStyle, @FontSize, @FontColor, @BackgroundColor, @MarginSize, @PaddingSize, @CustomCSS
	END

	CLOSE @campaignCursor
	DEALLOCATE @campaignCursor

	-- remove foreign key SaleCampaign_SaleCampaignCustomization
	ALTER TABLE [dbo].[SS_SC_SaleCampaign] DROP CONSTRAINT SaleCampaign_SaleCampaignCustomization;

	-- remove column [SaleCampaignCustomizationId]
	ALTER TABLE [dbo].[SS_SC_SaleCampaign] DROP COLUMN [SaleCampaignCustomizationId];

	-- drop table [SS_SC_SaleCampaignCustomization]
	DROP TABLE [SS_SC_SaleCampaignCustomization];

END

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO


-- ANYWHERE SLIDERS plugin - Nivo Slider
BEGIN TRANSACTION;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

-- Check if the Anywhere Sliders plugin is installed
IF(EXISTS (SELECT NULL FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SS_AS_NivoSettings]') AND type in (N'U')))
BEGIN
	-- fetch rows from the SS_AS_NivoSettings table
	DECLARE @nivoSlidersCursor CURSOR

	-- old table - declare the needed variables
	DECLARE @EntityTypeSlider INT
	DECLARE @NivoSliderId INT
	DECLARE @AutoSlideInterval INT
	DECLARE @AnimationSpeed INT
	DECLARE @EnableDirectionNavigation BIT
	DECLARE @DirectionNavigationShowOnHoverOnly BIT
	DECLARE @KeyboardNavigation BIT
	DECLARE @EnableControlNavigation BIT
	DECLARE @EnableControlNavigationThumbs BIT
	DECLARE @ThumbsBiggerSize INT
	DECLARE @CaptionOpacity REAL
	DECLARE @PrevText nvarchar(MAX)
	DECLARE @NextText nvarchar(MAX)
	DECLARE @Links BIT
	DECLARE @Width INT
	DECLARE @Height INT
	DECLARE @PauseOnHover BIT
	DECLARE @ShowCaption BIT
	DECLARE @Effect nvarchar(MAX)
	DECLARE @Slices INT
	DECLARE @BoxCols INT
	DECLARE @BoxRows INT
	DECLARE @Theme nvarchar(MAX)
	DECLARE @RandomStart BIT
	
	-- new table - declare the needed variables
	DECLARE @NewAutoSlideInterval nvarchar(MAX)
	DECLARE @NewAnimationSpeed nvarchar(MAX)
	DECLARE @NewEnableDirectionNavigation nvarchar(MAX)
	DECLARE @NewDirectionNavigationShowOnHoverOnly nvarchar(MAX)
	DECLARE @NewKeyboardNavigation nvarchar(MAX)
	DECLARE @NewEnableControlNavigation nvarchar(MAX)
	DECLARE @NewEnableControlNavigationThumbs nvarchar(MAX)
	DECLARE @NewThumbsBiggerSize nvarchar(MAX)
	DECLARE @NewCaptionOpacity nvarchar(MAX)
	DECLARE @NewLinks nvarchar(MAX)
	DECLARE @NewWidth nvarchar(MAX)
	DECLARE @NewHeight nvarchar(MAX)
	DECLARE @NewPauseOnHover nvarchar(MAX)
	DECLARE @NewShowCaption nvarchar(MAX)
	DECLARE @NewSlices nvarchar(MAX)
	DECLARE @NewBoxCols nvarchar(MAX)
	DECLARE @NewBoxRows nvarchar(MAX)
	DECLARE @NewRandomStart nvarchar(MAX)

	-- EntityType
	SET @EntityTypeSlider = 15;
	
	-- set the cursor
	SET @nivoSlidersCursor = CURSOR FOR
	SELECT [SliderId],[AutoSlideInterval],[AnimationSpeed],[EnableDirectionNavigation],[DirectionNavigationShowOnHoverOnly],[KeyboardNavigation],
	[EnableControlNavigation],[EnableControlNavigationThumbs],[ThumbsBiggerSize],[CaptionOpacity],[PrevText],[NextText],[Links],[Width],[Height],[PauseOnHover],
	[ShowCaption],[Effect],[Slices],[BoxCols],[BoxRows],[Theme],[RandomStart]
	FROM [dbo].[SS_AS_NivoSettings]
	
	-- open the cursor
	OPEN @nivoSlidersCursor
	
	-- initial fetch, so we can enter the while loop
	FETCH NEXT
	FROM @nivoSlidersCursor 
	INTO @NivoSliderId, @AutoSlideInterval, @AnimationSpeed, @EnableDirectionNavigation, @DirectionNavigationShowOnHoverOnly, @KeyboardNavigation, 
	@EnableControlNavigation, @EnableControlNavigationThumbs, @ThumbsBiggerSize, @CaptionOpacity, @PrevText, @NextText, @Links, @Width, @Height, @PauseOnHover,
	@ShowCaption, @Effect, @Slices, @BoxCols, @BoxRows, @Theme, @RandomStart

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP
	

		-- AutoSlideInterval
		SET @NewAutoSlideInterval = CAST(@AutoSlideInterval AS nvarchar(MAX));

		-- AnimationSpeed
		SET @NewAnimationSpeed = CAST(@AnimationSpeed AS nvarchar(MAX));

		-- EnableDirectionNavigation
		SET @NewEnableDirectionNavigation = dbo.ConvertToStringBool(@EnableDirectionNavigation);

		-- DirectionNavigationShowOnHoverOnly
		SET @NewDirectionNavigationShowOnHoverOnly = dbo.ConvertToStringBool(@DirectionNavigationShowOnHoverOnly);

		-- KeyboardNavigation
		SET @NewKeyboardNavigation = dbo.ConvertToStringBool(@KeyboardNavigation);

		-- EnableControlNavigation
		SET @NewEnableControlNavigation = dbo.ConvertToStringBool(@EnableControlNavigation);

		-- EnableControlNavigationThumbs
		SET @NewEnableControlNavigationThumbs = dbo.ConvertToStringBool(@EnableControlNavigationThumbs);

		-- ThumbsBiggerSize
		SET @NewThumbsBiggerSize = CAST(@ThumbsBiggerSize AS nvarchar(MAX));

		-- CaptionOpacity
		SET @NewCaptionOpacity = CAST(@CaptionOpacity AS nvarchar(MAX));

		-- Links
		SET @NewLinks = dbo.ConvertToStringBool(@Links);

		-- Width
		SET @NewWidth = CAST(@Width AS nvarchar(MAX));

		-- Height
		SET @NewHeight = CAST(@Height AS nvarchar(MAX));

		-- PauseOnHover
		SET @NewPauseOnHover = dbo.ConvertToStringBool(@PauseOnHover);

		-- ShowCaption
		SET @NewShowCaption = dbo.ConvertToStringBool(@ShowCaption);

		-- Slices
		SET @NewSlices = CAST(@Slices AS nvarchar(MAX));

		-- BoxCols
		SET @NewBoxCols = CAST(@BoxCols AS nvarchar(MAX));

		-- BoxRows
		SET @NewBoxRows = CAST(@BoxRows AS nvarchar(MAX));

		-- RandomStart
		SET @NewRandomStart = dbo.ConvertToStringBool(@RandomStart);
		


		-- Insert into entity settings
		INSERT INTO [dbo].[SS_ES_EntitySetting]
		([EntityType],[EntityId],[Key],[Value])
		VALUES
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.autoslideinterval', @NewAutoSlideInterval),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.animationspeed', @NewAnimationSpeed),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.enabledirectionnavigation', @NewEnableDirectionNavigation),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.directionnavigationshowonhoveronly', @NewDirectionNavigationShowOnHoverOnly),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.keyboardnavigation', @NewKeyboardNavigation),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.enablecontrolnavigation', @NewEnableControlNavigation),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.enablecontrolnavigationthumbs', @NewEnableControlNavigationThumbs),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.thumbsbiggersize', @NewThumbsBiggerSize),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.captionopacity', @NewCaptionOpacity),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.prevtext', @PrevText),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.nexttext', @NextText),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.links', @NewLinks),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.width', @NewWidth),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.height', @NewHeight),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.pauseonhover', @NewPauseOnHover),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.showcaption', @NewShowCaption),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.effect', @Effect),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.slices', @NewSlices),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.boxcols', @NewBoxCols),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.boxrows', @NewBoxRows),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.theme', @Theme),
		(@EntityTypeSlider, @NivoSliderId, 'nivosettings.randomstart', @NewRandomStart)
		


		-- fetch the next row
		FETCH NEXT
		FROM @nivoSlidersCursor 
		INTO @NivoSliderId, @AutoSlideInterval, @AnimationSpeed, @EnableDirectionNavigation, @DirectionNavigationShowOnHoverOnly, @KeyboardNavigation, 
		@EnableControlNavigation, @EnableControlNavigationThumbs, @ThumbsBiggerSize, @CaptionOpacity, @PrevText, @NextText, @Links, @Width, @Height, @PauseOnHover,
		@ShowCaption, @Effect, @Slices, @BoxCols, @BoxRows, @Theme, @RandomStart
	END

	CLOSE @nivoSlidersCursor
	DEALLOCATE @nivoSlidersCursor

	-- remove foreign key NivoSettings_Slider
	ALTER TABLE [dbo].[SS_AS_NivoSettings] DROP CONSTRAINT NivoSettings_Slider;
	
	-- drop table [SS_AS_NivoSettings]
	DROP TABLE [SS_AS_NivoSettings];

END

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO


-- ANYWHERE SLIDERS plugin - Carousel 2D Slider
BEGIN TRANSACTION;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

-- Check if the Anywhere Sliders plugin is installed
IF(EXISTS (SELECT NULL FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SS_AS_CarouselSettings]') AND type in (N'U')))
BEGIN
	-- fetch rows from the [SS_AS_CarouselSettings] table
	DECLARE @carousel2dSliderCursor CURSOR

	-- old table - declare the needed variables
	DECLARE @EntityTypeSlider2D INT
	DECLARE @Carousel2dSliderId INT
	DECLARE @AutoSlideInterval INT
	DECLARE @Navigation BIT
	DECLARE @Links BIT
	DECLARE @Width INT
	DECLARE @Height INT
	DECLARE @HoverPause BIT
	DECLARE @ShowTitle BIT
	
	-- new table - declare the needed variables
	DECLARE @NewAutoSlideInterval nvarchar(MAX)
	DECLARE @NewNavigation nvarchar(MAX)
	DECLARE @NewLinks nvarchar(MAX)
	DECLARE @NewWidth nvarchar(MAX)
	DECLARE @NewHeight nvarchar(MAX)
	DECLARE @NewHoverPause nvarchar(MAX)
	DECLARE @NewShowTitle nvarchar(MAX)

	-- EntityType
	SET @EntityTypeSlider2D = 15;
	
	-- set the cursor
	SET @carousel2dSliderCursor = CURSOR FOR
	SELECT [SliderId],[AutoSlideInterval],[Navigation],[Links],[Width],[Height],[HoverPause],[ShowTitle]
	FROM [dbo].[SS_AS_CarouselSettings]
	
	-- open the cursor
	OPEN @carousel2dSliderCursor
	
	-- initial fetch, so we can enter the while loop
	FETCH NEXT
	FROM @carousel2dSliderCursor 
	INTO @Carousel2dSliderId, @AutoSlideInterval, @Navigation, @Links, @Width, @Height, @HoverPause, @ShowTitle

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP
	

		-- AutoSlideInterval
		SET @NewAutoSlideInterval = CAST(@AutoSlideInterval AS nvarchar(MAX));

		-- Navigation
		SET @NewNavigation = dbo.ConvertToStringBool(@Navigation);

		-- Links
		SET @NewLinks = dbo.ConvertToStringBool(@Links);

		-- Width
		SET @NewWidth = CAST(@Width AS nvarchar(MAX));

		-- Height
		SET @NewHeight = CAST(@Height AS nvarchar(MAX));

		-- HoverPause
		SET @NewHoverPause = dbo.ConvertToStringBool(@HoverPause);

		-- ShowTitle
		SET @NewShowTitle = dbo.ConvertToStringBool(@ShowTitle);
		


		-- Insert into entity settings
		INSERT INTO [dbo].[SS_ES_EntitySetting]
		([EntityType],[EntityId],[Key],[Value])
		VALUES
		(@EntityTypeSlider2D, @Carousel2dSliderId, 'carouselsettings.autoslideinterval', @NewAutoSlideInterval),
		(@EntityTypeSlider2D, @Carousel2dSliderId, 'carouselsettings.navigation', @NewNavigation),
		(@EntityTypeSlider2D, @Carousel2dSliderId, 'carouselsettings.links', @NewLinks),
		(@EntityTypeSlider2D, @Carousel2dSliderId, 'carouselsettings.width', @NewWidth),
		(@EntityTypeSlider2D, @Carousel2dSliderId, 'carouselsettings.height', @NewHeight),
		(@EntityTypeSlider2D, @Carousel2dSliderId, 'carouselsettings.hoverpause', @NewHoverPause),
		(@EntityTypeSlider2D, @Carousel2dSliderId, 'carouselsettings.showtitle', @NewShowTitle)
		


		-- fetch the next row
		FETCH NEXT
		FROM @carousel2dSliderCursor 
		INTO @Carousel2dSliderId, @AutoSlideInterval, @Navigation, @Links, @Width, @Height, @HoverPause, @ShowTitle
	END

	CLOSE @carousel2dSliderCursor
	DEALLOCATE @carousel2dSliderCursor

	-- remove foreign key CarouselSettings_Slider
	--ALTER TABLE [dbo].[SS_AS_CarouselSettings] DROP CONSTRAINT CarouselSettings_Slider;
	
	-- drop table [SS_AS_CarouselSettings]
	DROP TABLE [SS_AS_CarouselSettings];

END

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO


-- ANYWHERE SLIDERS plugin - Carousel 3D Slider
BEGIN TRANSACTION;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

-- Check if the Anywhere Sliders plugin is installed
IF(EXISTS (SELECT NULL FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SS_AS_Carousel3DSettings]') AND type in (N'U')))
BEGIN
	-- fetch rows from the [SS_AS_Carousel3DSettings] table
	DECLARE @carousel3dSliderCursor CURSOR

	-- old table - declare the needed variables
	DECLARE @EntityTypeSlider3D INT
	DECLARE @Carousel3dSliderId INT
	DECLARE @Width INT
	DECLARE @Height INT
	DECLARE @PictureWidth INT
	DECLARE @PictureHeight INT
	DECLARE @YRadius INT
	DECLARE @XPosition INT
	DECLARE @YPosition INT
	DECLARE @Speed INT
	DECLARE @MouseWheel INT
	DECLARE @AutoRotateDelay INT
	DECLARE @AutoRotate BIT
	
	-- new table - declare the needed variables
	DECLARE @NewWidth nvarchar(MAX)
	DECLARE @NewHeight nvarchar(MAX)
	DECLARE @NewPictureWidth nvarchar(MAX)
	DECLARE @NewPictureHeight nvarchar(MAX)
	DECLARE @NewYRadius nvarchar(MAX)
	DECLARE @NewXPosition nvarchar(MAX)
	DECLARE @NewYPosition nvarchar(MAX)
	DECLARE @NewSpeed nvarchar(MAX)
	DECLARE @NewMouseWheel nvarchar(MAX)
	DECLARE @NewAutoRotateDelay nvarchar(MAX)
	DECLARE @NewAutoRotate nvarchar(MAX)

	-- EntityType
	SET @EntityTypeSlider3D = 15;
	
	-- set the cursor
	SET @carousel3dSliderCursor = CURSOR FOR
	SELECT [SliderId],[Width],[Height],[PictureWidth],[PictureHeight],[YRadius],[XPosition],[YPosition],[Speed],[MouseWheel],[AutoRotateDelay],[AutoRotate]	
	FROM [dbo].[SS_AS_Carousel3DSettings]
	
	-- open the cursor
	OPEN @carousel3dSliderCursor
	
	-- initial fetch, so we can enter the while loop
	FETCH NEXT
	FROM @carousel3dSliderCursor 
	INTO @Carousel3dSliderId, @Width, @Height, @PictureWidth, @PictureHeight, @YRadius, @XPosition, @YPosition, @Speed, @MouseWheel, @AutoRotateDelay, @AutoRotate

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP
	

		-- Width
		SET @NewWidth = CAST(@Width AS nvarchar(MAX));

		-- Height
		SET @NewHeight = CAST(@Height AS nvarchar(MAX));

		-- PictureWidth
		SET @NewPictureWidth = CAST(@PictureWidth AS nvarchar(MAX));

		-- PictureHeight
		SET @NewPictureHeight = CAST(@PictureHeight AS nvarchar(MAX));

		-- YRadius
		SET @NewYRadius = CAST(@YRadius AS nvarchar(MAX));

		-- XPosition
		SET @NewXPosition = CAST(@XPosition AS nvarchar(MAX));

		-- YPosition
		SET @NewYPosition = CAST(@YPosition AS nvarchar(MAX));

		-- Speed
		SET @NewSpeed = CAST(@Speed AS nvarchar(MAX));

		-- MouseWheel
		SET @NewMouseWheel = CAST(@MouseWheel AS nvarchar(MAX));

		-- AutoRotateDelay
		SET @NewAutoRotateDelay = CAST(@AutoRotateDelay AS nvarchar(MAX));

		-- AutoRotate
		SET @NewAutoRotate = dbo.ConvertToStringBool(@AutoRotate);
		


		-- Insert into entity settings
		INSERT INTO [dbo].[SS_ES_EntitySetting]
		([EntityType],[EntityId],[Key],[Value])
		VALUES
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.width', @NewWidth),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.height', @NewHeight),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.picturewidth', @NewPictureWidth),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.pictureheight', @NewPictureHeight),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.yradius', @NewYRadius),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.xposition', @NewXPosition),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.yposition', @NewYPosition),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.speed', @NewSpeed),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.mousewheel', @NewMouseWheel),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.autorotatedelay', @NewAutoRotateDelay),
		(@EntityTypeSlider3D, @Carousel3dSliderId, 'carousel3dsettings.autorotate', @NewAutoRotate)
		


		-- fetch the next row
		FETCH NEXT
		FROM @carousel3dSliderCursor 
	INTO @Carousel3dSliderId, @Width, @Height, @PictureWidth, @PictureHeight, @YRadius, @XPosition, @YPosition, @Speed, @MouseWheel, @AutoRotateDelay, @AutoRotate
	END

	CLOSE @carousel3dSliderCursor
	DEALLOCATE @carousel3dSliderCursor

	-- remove foreign key CarouselSettings_Slider
	--ALTER TABLE [dbo].[SS_AS_Carousel3DSettings] DROP CONSTRAINT Carousel3DSettings_SLider;
	
	-- drop table [SS_AS_Carousel3DSettings]
	DROP TABLE [SS_AS_Carousel3DSettings];

END

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO


-- JCarousel plugin - SP for dropping columns
IF OBJECT_ID ( 'dbo.DropJCarouselColumn', 'P' ) IS NOT NULL 
DROP PROCEDURE dbo.DropJCarouselColumn;
GO

CREATE PROCEDURE dbo.DropJCarouselColumn @ColumnName nvarchar(100)
AS
	IF((SELECT column_id FROM sys.columns WHERE NAME = N'' + @ColumnName AND object_id = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]')) IS NOT NULL)
		EXEC('ALTER TABLE [dbo].[SS_JC_JCarousel] DROP COLUMN ' + @ColumnName)
GO

-- JCarousel plugin
BEGIN TRANSACTION;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

-- Check if the JCarousel plugin is installed
IF(EXISTS (SELECT NULL FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]') AND type in (N'U'))
	AND EXISTS (SELECT NULL FROM sys.columns WHERE NAME = N'NumberOfItems' AND object_id = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]')))
BEGIN
	-- fetch rows from the [SS_JC_JCarousel] table
	DECLARE @jCarouselCursor CURSOR

	-- old table - declare the needed variables
	DECLARE @EntityTypeCarousel INT
	DECLARE @CarouselId INT
	DECLARE @NumberOfItems INT
	DECLARE @NumberOfVisibleItems INT
	DECLARE @JCarouselItemMinWidth INT
	DECLARE @Skin nvarchar(MAX)
	DECLARE @ImageSize INT
	DECLARE @ShowItemsName BIT
	DECLARE @ShowTitle BIT
	DECLARE @ShowProductsPrice BIT
	DECLARE @ShowProductsOldPrice BIT
	DECLARE @ShowShortDescription BIT
	DECLARE @ShowDetailsButton BIT
	DECLARE @ShowRatings BIT
	DECLARE @CarouselOrientation BIT
	DECLARE @StartIndex INT
	DECLARE @ScrollItems INT
	DECLARE @Easing nvarchar(MAX)
	DECLARE @Autoscroll INT
	DECLARE @WrapItems nvarchar(MAX)
	DECLARE @AnimationSpeed nvarchar(MAX)
	
	-- new table - declare the needed variables
	DECLARE @NewNumberOfItems nvarchar(MAX)
	DECLARE @NewNumberOfVisibleItems nvarchar(MAX)
	DECLARE @NewJCarouselItemMinWidth nvarchar(MAX)
	DECLARE @NewImageSize nvarchar(MAX)
	DECLARE @NewShowItemsName nvarchar(MAX)
	DECLARE @NewShowTitle nvarchar(MAX)
	DECLARE @NewShowProductsPrice nvarchar(MAX)
	DECLARE @NewShowProductsOldPrice nvarchar(MAX)
	DECLARE @NewShowShortDescription nvarchar(MAX)
	DECLARE @NewShowDetailsButton nvarchar(MAX)
	DECLARE @NewShowRatings nvarchar(MAX)
	DECLARE @NewCarouselOrientation nvarchar(MAX)
	DECLARE @NewStartIndex nvarchar(MAX)
	DECLARE @NewScrollItems nvarchar(MAX)
	DECLARE @NewAutoscroll nvarchar(MAX)

	-- EntityType
	SET @EntityTypeCarousel = 35;
	
	EXEC sp_executesql N'
		SET @jCarouselCursor = CURSOR FOR
		SELECT [Id],[NumberOfItems],[NumberOfVisibleItems],[JCarouselItemMinWidth],[Skin],[ImageSize],[ShowItemsName],[ShowTitle],[ShowProdictsPrice],[ShowProdictsOldPrice],
		[ShowShortDescription],[ShowDetailsButton],[ShowRatings],[CarouselOrientation],[StartIndex],[ScrollItems],[Easing],[Autoscroll],[WrapItems],[AnimationSpeed]
		FROM [dbo].[SS_JC_JCarousel]
		OPEN @jCarouselCursor', 
		N'@jCarouselCursor CURSOR OUTPUT', 
		@jCarouselCursor OUTPUT;
	
	-- initial fetch, so we can enter the while loop
	FETCH NEXT
	FROM @jCarouselCursor 
	INTO @CarouselId, @NumberOfItems, @NumberOfVisibleItems, @JCarouselItemMinWidth, @Skin, @ImageSize, @ShowItemsName, @ShowTitle, @ShowProductsPrice, @ShowProductsOldPrice,
	@ShowShortDescription, @ShowDetailsButton, @ShowRatings, @CarouselOrientation, @StartIndex, @ScrollItems, @Easing, @Autoscroll, @WrapItems, @AnimationSpeed

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP ++ MAIN LOOP
	
	
		-- NumberOfItems
		SET @NewNumberOfItems = CAST(@NumberOfItems AS nvarchar(MAX));

		-- NumberOfVisibleItems
		SET @NewNumberOfVisibleItems = CAST(@NumberOfVisibleItems AS nvarchar(MAX));
		
		-- JCarouselItemMinWidth
		SET @NewJCarouselItemMinWidth = CAST(@JCarouselItemMinWidth AS nvarchar(MAX));
		
		-- ImageSize
		SET @NewImageSize = CAST(@ImageSize AS nvarchar(MAX));
		
		-- ShowItemsName
		SET @NewShowItemsName = dbo.ConvertToStringBool(@ShowItemsName);
		
		-- ShowTitle
		SET @NewShowTitle = dbo.ConvertToStringBool(@ShowTitle);
		
		-- ShowProductsPrice
		SET @NewShowProductsPrice = dbo.ConvertToStringBool(@ShowProductsPrice);
		
		-- ShowProductsOldPrice
		SET @NewShowProductsOldPrice = dbo.ConvertToStringBool(@ShowProductsOldPrice);
		
		-- ShowShortDescription
		SET @NewShowShortDescription = dbo.ConvertToStringBool(@ShowShortDescription);
		
		-- ShowDetailsButton
		SET @NewShowDetailsButton = dbo.ConvertToStringBool(@ShowDetailsButton);
		
		-- ShowRatings
		SET @NewShowRatings = dbo.ConvertToStringBool(@ShowRatings);
		
		-- CarouselOrientation
		SET @NewCarouselOrientation = dbo.ConvertToStringBool(@CarouselOrientation);
		
		-- StartIndex
		SET @NewStartIndex = CAST(@StartIndex AS nvarchar(MAX));
		
		-- ScrollItems
		SET @NewScrollItems = CAST(@ScrollItems AS nvarchar(MAX));
		
		-- Autoscroll
		SET @NewAutoscroll = CAST(@Autoscroll AS nvarchar(MAX));
		
		
		-- Insert into entity settings
		INSERT INTO [dbo].[SS_ES_EntitySetting]
		([EntityType],[EntityId],[Key],[Value])
		VALUES
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.numberofitems', @NewNumberOfItems),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.numberofvisibleitems', @NewNumberOfVisibleItems),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.jcarouselitemminwidth', @NewJCarouselItemMinWidth),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.skin', @Skin),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.imagesize', @NewImageSize),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.showitemsname', @NewShowItemsName),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.showtitle', @NewShowTitle),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.showproductsprice', @NewShowProductsPrice),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.showproductsoldprice', @NewShowProductsOldPrice),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.showshortdescription', @NewShowShortDescription),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.showdetailsbutton', @NewShowDetailsButton),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.showratings', @NewShowRatings),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.carouselorientation', @NewCarouselOrientation),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.startindex', @NewStartIndex),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.scrollitems', @NewScrollItems),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.easing', @Easing),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.autoscroll', @NewAutoscroll),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.wrapitems', @WrapItems),
		(@EntityTypeCarousel, @CarouselId, 'jcarouselentitysettings.animationspeed', @AnimationSpeed)
		


		-- fetch the next row
		FETCH NEXT
		FROM @jCarouselCursor 
		INTO @CarouselId, @NumberOfItems, @NumberOfVisibleItems, @JCarouselItemMinWidth, @Skin, @ImageSize, @ShowItemsName, @ShowTitle, @ShowProductsPrice, @ShowProductsOldPrice,
		@ShowShortDescription, @ShowDetailsButton, @ShowRatings, @CarouselOrientation, @StartIndex, @ScrollItems, @Easing, @Autoscroll, @WrapItems, @AnimationSpeed
	END

	CLOSE @jCarouselCursor
	DEALLOCATE @jCarouselCursor

	-- drop constraints
	DECLARE @ConstraintName nvarchar(200);

	SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS
	WHERE PARENT_OBJECT_ID = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]')
	AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns
							WHERE NAME = N'JCarouselItemMinWidth'
							AND object_id = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]'))

	IF @ConstraintName IS NOT NULL
	EXEC('ALTER TABLE [dbo].[SS_JC_JCarousel] DROP CONSTRAINT ' + @ConstraintName)

	SET @ConstraintName = NULL;

	SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS
	WHERE PARENT_OBJECT_ID = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]')
	AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns
							WHERE NAME = N'ShowRatings'
							AND object_id = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]'))

	IF @ConstraintName IS NOT NULL
	EXEC('ALTER TABLE [dbo].[SS_JC_JCarousel] DROP CONSTRAINT ' + @ConstraintName)

	-- remove some columns
	EXEC dbo.DropJCarouselColumn @ColumnName = 'NumberOfItems'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'NumberOfVisibleItems'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'JCarouselItemMinWidth'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'Skin'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ImageSize'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ShowItemsName'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ShowTitle'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ShowProdictsPrice'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ShowProdictsOldPrice'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ShowShortDescription'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ShowDetailsButton'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ShowRatings'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'CarouselOrientation'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'RightToLeft'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'StartIndex'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'ScrollItems'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'Easing'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'Autoscroll'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'WrapItems'
	EXEC dbo.DropJCarouselColumn @ColumnName = 'AnimationSpeed'
	
	IF OBJECT_ID ( 'dbo.DropJCarouselColumn', 'P' ) IS NOT NULL 
	EXEC('DROP PROCEDURE dbo.DropJCarouselColumn;');

	-- add new column - "CarouselType"
	ALTER TABLE [dbo].[SS_JC_JCarousel] ADD [CarouselType] INT NOT NULL DEFAULT 0;
	
	-- remove the constraint for the new column - "CarouselType"
	SET @ConstraintName = NULL;

	SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS
	WHERE PARENT_OBJECT_ID = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]')
	AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns
							WHERE NAME = N'CarouselType'
							AND object_id = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]'))

	IF @ConstraintName IS NOT NULL
	EXEC('ALTER TABLE [dbo].[SS_JC_JCarousel] DROP CONSTRAINT ' + @ConstraintName)
	
	-- add new column - "DataSourceEntityId"
	ALTER TABLE [dbo].[SS_JC_JCarousel] ADD [DataSourceEntityId] INT NOT NULL DEFAULT 0;
	
	-- remove the constraint for the new column - "DataSourceEntityId"
	SET @ConstraintName = NULL;

	SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS
	WHERE PARENT_OBJECT_ID = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]')
	AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns
							WHERE NAME = N'DataSourceEntityId'
							AND object_id = OBJECT_ID(N'[dbo].[SS_JC_JCarousel]'))

	IF @ConstraintName IS NOT NULL
	EXEC('ALTER TABLE [dbo].[SS_JC_JCarousel] DROP CONSTRAINT ' + @ConstraintName)
	
END

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO

IF OBJECT_ID ( N'dbo.ConvertToStringBool', N'FN' ) IS NOT NULL 
	DROP FUNCTION dbo.ConvertToStringBool;
GO


---------------- COLOR PRESETS UPGRADE SCRIPT --------------------

BEGIN TRANSACTION;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

IF OBJECT_ID('tempdb..#colors') IS NOT NULL DROP TABLE [#colors]

create table #colors (
	ThemeName nvarchar(max),
	Name nvarchar(max),
	Hex nvarchar(max)
)

-- Currently just for Lavella, but this should contain all the different colors for all themes
insert into [#colors] (ThemeName, Name, Hex) values
	-- Lavella --
	('lavella', 'Avocado', '9aba6b'),
	('lavella', 'Lemon', 'd19434'),
	('lavella', 'Lavender', '895c92'),
	-- End Lavella --

	-- Allure --
	('allure', 'Peach', 'e87772'),
	('allure', 'Orange', 'f3a078'),
	('allure', 'Olive', 'a6b773'),
	('allure', 'Sky', '87badd'),
	('allure', 'Wood', 'b19489'),
	('allure', 'Violet', 'a9a1c6'),
	-- End Allure --

	-- Art Factory --
	('artfactory', 'Aquamarine', '8bc8ca'),
	('artfactory', 'Mint', '93d8a8'),
	('artfactory', 'Bubblegum', 'ff9b9b'),
	-- End Art Factory --

	-- Brooklyn --
	('brooklyn', 'Rose', 'cc8a97'),
	('brooklyn', 'Fresh', '76b79b'),
	('brooklyn', 'Lilac', 'a58ab4'),
	('brooklyn', 'Tomato', 'de524e'),
	('brooklyn', 'Sunflower', 'f2c242'),
	('brooklyn', 'Neutral', '999999'),
	('brooklyn', 'Graphite', '2a2a2a'),
	('brooklyn', 'Sky', '83b9e3'),
	-- End Brooklyn --

	-- Motion --
	('motion', 'Nature', '46c688'),
	('motion', 'Power', 'f05d61'),
	('motion', 'Light', 'f7b645'),
	('motion', 'Sky', '3cc0d3'),
	('motion', 'Neutral', '4a4a4a'),
	-- End Motion --

	-- Native --
	('native', 'DeepSea', '08766b'),
	('native', 'Frost', '6981c1'),
	('native', 'Plum', '6e4b83'),
	('native', 'Pomegranate', 'f3594d'),
	('native', 'Mint', '75bfa1'),
	-- End Native --

	-- Nitro --
	('nitro', 'Red', 'aa1122'),
	('nitro', 'Green', '0b7060'),
	('nitro', 'Ohra', 'd78146'),
	('nitro', 'Blue', '4875b9'),
	('nitro', 'Neutral', '333333'),
	-- End Nitro --

	-- Smart --
	('smart', 'Blueberry', '843a4b'),
	('smart', 'Sand', 'b6a287'),
	('smart', 'Ocean', '5be'),
	('smart', 'Violet', '8999cc'),
	('smart', 'Fresh', '74b841'),
	-- End Smart --

	-- Tiffany --
	('tiffany', 'Chocolate', '8e807a'),
	('tiffany', 'Sugar', 'a18a76'),
	('tiffany', 'Dove', '777'),
	-- End Tiffany --
	
	-- Traction --
	('traction', 'Red', 'c03'),
	('traction', 'Black', '333'),
	('traction', 'Grey', 'aaa'),
	('traction', 'Orange', 'e04900'),
	('traction', 'Blue', '467bb1'),
	('traction', 'Yellow', 'ffbb34')
	-- End Traction --
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.UpdateSettingsForTheme'))
   drop procedure [dbo].[UpdateSettingsForTheme]
GO

CREATE PROCEDURE dbo.UpdateSettingsForTheme (@ThemeName nvarchar(max), @DefaultColor nvarchar(max))
AS

	Update [dbo].[Setting]
	set Value = (
			COALESCE(
				COALESCE(
					(select Hex from [#colors] where Name COLLATE DATABASE_DEFAULT = [dbo].[Setting].[Value] COLLATE DATABASE_DEFAULT and ThemeName COLLATE DATABASE_DEFAULT = @ThemeName COLLATE DATABASE_DEFAULT),
					(select Hex from [#colors] where Hex COLLATE DATABASE_DEFAULT = [dbo].[Setting].[Value] COLLATE DATABASE_DEFAULT and ThemeName COLLATE DATABASE_DEFAULT = @ThemeName COLLATE DATABASE_DEFAULT)
				), 
				@DefaultColor)
		)
	where Name like '%' + @ThemeName + 'themesettings.preset%'

GO

-- Call for all themes
exec dbo.UpdateSettingsForTheme @ThemeName = 'lavella', @DefaultColor = '9ca34e'
exec dbo.UpdateSettingsForTheme @ThemeName = 'allure', @DefaultColor = 'e87772'
exec dbo.UpdateSettingsForTheme @ThemeName = 'artfactory', @DefaultColor = '8bc8ca'
exec dbo.UpdateSettingsForTheme @ThemeName = 'brooklyn', @DefaultColor = 'cc8a97'
exec dbo.UpdateSettingsForTheme @ThemeName = 'motion', @DefaultColor = '46c688'
exec dbo.UpdateSettingsForTheme @ThemeName = 'native', @DefaultColor = '08766b'
exec dbo.UpdateSettingsForTheme @ThemeName = 'nitro', @DefaultColor = 'aa1122'
exec dbo.UpdateSettingsForTheme @ThemeName = 'smart', @DefaultColor = '843a4b'
exec dbo.UpdateSettingsForTheme @ThemeName = 'tiffany', @DefaultColor = '96806d'

-- For every theme
-- Select dbo.UpdateSettingsForTheme('theme name', 'default color value')

IF OBJECT_ID('tempdb..#colors') IS NOT NULL DROP TABLE [#colors]

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.UpdateSettingsForTheme'))
   drop procedure [dbo].[UpdateSettingsForTheme]
GO

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO

------------- END COLOR PRESETS UPGRADE SCRIPT --------------------
