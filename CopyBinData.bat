rmdir /s /q "C:\dev\Sprue\SprueKit\bin\Debug\Content"
rmdir /s /q "C:\dev\Sprue\SprueKit\bin\Release\Content"
xcopy /s "C:\dev\Sprue\SprueKit\Content\bin" "C:\dev\Sprue\SprueKit\bin\Release\Content\"
xcopy /s "C:\dev\Sprue\SprueKit\Content\bin" "C:\dev\Sprue\SprueKit\bin\Debug\Content\"