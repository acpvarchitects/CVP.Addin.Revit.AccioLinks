   M S B A M L   `   `   `  ????E  AAccioLinks, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:AccioLinks
AccioLinks  5          AccioLinks.UserControl   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      *localclr-namespace:AccioLinks     5        ? Title$  Accio Links??5       ? 
ResizeMode$ CanResizeWithGrip=?6   $	??500??64   $	??280??6C   $??850         ??6S   $	??550??5	      
???????.??5      ? $??10,0,0,0q?6    ? ColumnDefinitions 5   
   ?? 5      $	??300??6   ?? 5      $	??200 ?6   $	??200??6+    ? RowDefinitions 5   
   ?? 5      $??50 ?6   ?? 5      $
??Auto ?6   $??20??6*   ?? 5      $	??21* ?6   .+?5   
   ?? -   5         ??Name$ txtboxFolderPath??5      $
??0,10q?6,   $??Center=?6:   6W   ]? 5   
   $??1??6   $??0,10,0,5q?6!   .+?5      ?? -   5         ??Name$ btnCheckAll??5      $??80??6D   .??6O   	Check All6Z   ?? 5      -   5        $ btnUnheckAll??5      $??80??6F   $??10,0,0,0q?6Q   .??6c   Uncheck All6p   ?? 5      -   5        $ 	btnRemove??5      $	??120??6@   $??Right=?6L   $??Right=?6c   .??6   Remove Selected6?   5      ]? 5   
   $??0??6   $??1??6!    J? RenderTransformOrigin$ 0.5,0.5&?61   ??.  $??Center=?6g   $	??200??6?   $??0q?6?   .+?5      ?? -   5        $ BrowseButton??5      $??
10,10,5,10q?6F   $??90??6Z   .??6e   Browse6m   ?? 5      -   5        $ btnOpenFolder??5      $??	2,10,5,10q?6H   $??85??6[   .??6f   Open Folder6s   5      ?? 5   
   -   5         ??Name$ 	fileNames??5      $??2??6%    ?? SelectionMode$ Multiple=?62   	 J? 	AllowDrop	 . ??6K   ?? 
 ?? Path$
 LinkcheckboxesList???5       ?? 5!      .R?5"      ??  ?? Orientation$ 
Horizontal=?6!   .+?5#      ?? -   5         d? 	IsChecked 5#   #   ?? $
 	IsChecked? ?? Mode$ TwoWay=?6T   ?? 5$      $??5,0,0,0q?6B   ??6$   ?? $
 ChkName?6B   5%      5&      5(      ?? 5)   
   $??2??6   $??1??6    $??10,0,0,0q?60   $??Link Positioning??6B   $??90??6\   $	??Top=?6h   .??5*      ?? .+?5+      	? -	   5         	?Name$ 	btnShared??5+       	? 	GroupName$ Positioning??61   $??0,20,0,0q?6I   .??6[   By Shared Coordinates6r   	? 5,      -
   5        $ 	btnOrigin??5,      $ Positioning??61   .??6I   	By Origin6T   5-      5.      ]? 5/   
   $??2??6   $??1??6!   $??Bottom=?61   ??.  $??Center=?6b   $??50??6   $	??200??6?   .+?50      ?? -   5        $ AddLinks??50      $??90??6>   $??5q?6I   .??6U   	Add Links6`   ?? 51      -   5        $ Cancel??51      $??90??6:   $??5q?6E   .??6P   Cancel6X   52      53      54      