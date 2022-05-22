jump start
me_maybe proc 
	mov r2, 1
	ret
me_maybe endp
start:
	call me_maybe
end start
halt