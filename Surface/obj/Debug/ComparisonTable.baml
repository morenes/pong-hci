   M S B A M L   `   `   `  ����  ItemCompare!    ItemCompare.ComparisonTable   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      e aMicrosoft.Surface.Presentation, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e351s)http://schemas.microsoft.com/surface/2008  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5      -   5        $��TwoItemComparisonTable��5      $	��726��5      $	��726��5      .��5
      � -   5        $
��grid��5
   	     � RowDefinitions  5      �� 5      $��* �6   �� 5      $��2* �6   �� 5      $��* �6    � ColumnDefinitions 5      �� 5      $��* �6   �� 5      $	��260 �6   �� 5      $��17 �6   �� 5      $	��260 �6   �� 5      $��* �6   .+�5      �� -   5         ��Name$ TetherSpaceBinding��5      
������ $��0��5      $��1��5      {�.   J� IsHitTestVisible .  $
��Auto��5      $
��Auto��5       A� 5!         GlowRing#��[  6   $��5��5"      $��0��5#      $��3��5&      ��5$      ��  �� Path$ ActualHeight� �� ElementName$
 grid�� �� Mode$ Default=���5%      �� $ ActualHeight�$
 grid��$ Default=�5&      � 5'      $��0��6   $��3��5(      $��5��5)      $��Stretch=�5,      ��5*      �� $ ActualHeight�$
 grid��$ Default=���5+      �� $ ActualHeight�$
 grid��$ Default=�  5-      �� 5.   
   $��* �6   �� 5/   
   $��2* �6   �� 50   
   $��* �6   .+�52      �� $
��Left=�6   $��Stretch=�53      $
��Auto��55       J� Opacity$	 0.2Q�56      $��1��57      
������ ��54      �� $ ActualWidth�$ TetherSpaceBinding��$ Default=�58      �� 59      $��Stretch=�6   $��Right=�5:      $
��Auto��5;      $	 0.2Q�5<      $��1��5=       J� RenderTransformOrigin$ 0.5,0.5&�5>      $��2��5?      
������ ��5@      �� $ ActualWidth�$ TetherSpaceBinding��$ Default=�5A      5B      �� 5C        ItemBackgroundFill#��[ 6   $��Stretch=�5D      $
��Auto��5E      $��1��5F      $��1��5G      $��1��5H      $��1��5I      �� 5J      #��[ 6   $��Stretch=�5K      $
��Auto��5L      $��1��5M      $��3��5N      $��1��5O      $��1��5P      	 J� Effect	 5Q      �� $ Effect�$ LeftBackground��$ Default=�A  ;Microsoft.Surface.Presentation.Controls.SurfaceScrollViewer  5R      $��Hidden=�6   $��Disabled=�5S      $��3��5T      $��1��5U      $��1��5V      .��5W      � -   5        
 �Name$
 RowHost��5W      $
��Auto��5X      $
��Auto��5Y      5Z      �� 5\      -   5         ��Name$ 
ItemBrand2��5\        CompareBrandTextStyle#��[ 5]      $��Bottom=�5^      $��	BrandText��5_      $y�Visible=�5`      $��3��5a      $��Stretch=�5b      $��1��5c      $��0,0,0,35q�5d      �� 5f      -   5        $ 	ItemName2��5f         CompareHeadingLabelTextStyle#��[ 5g      $��Bottom=�5h      $��SomePlaceHolderText��5i      $y�Visible=�5j      $��3��5k      $��Stretch=�5l      $��1��5m      $��0,0,0,3q�5n      �� 5p      -   5        $ 
ItemBrand1��5p      #��[ 5q      $��Bottom=�5r      $��	BrandText��5s      $y�Visible=�5t      $��1��5u      $��Stretch=�5v      $��1��5w      $��0,0,0,32q�5x      �� 5z      -   5        $ 	ItemName1��5z      #��[ 5{      $��Bottom=�5|      $��SomePlaceHolderText��5}      $y�Visible=�5~      $��Stretch=�5      $��1��5�      $��0,0,0,3q�5�      $��1��5�      5�      5�      