import tkinter

window_width = 640
window_height = 720
def GetWindowPos():
	# global x,y
	# x = (ws/2) - (window_width/2)
	# y = (hs/2) - (window_height/2)
	win.bind_all('<Configure>', HoldOn)
def HoldOn(event):
	# ws = win.winfo_screenwidth()
	# hs = win.winfo_screenheight()
	# x = (ws/2) - (window_width/2)
	# y = (hs/2) - (window_height/2)
	win.geometry('%dx%d+%d+%d' % (window_width, window_height, x, y))
global x,y
win = tkinter.Tk()
ws = win.winfo_screenwidth()
hs = win.winfo_screenheight()
x = (ws/2) - (window_width/2)
y = (hs/2) - (window_height/2)
win.geometry('%dx%d+%d+%d' % (window_width, window_height, x, y))
#win.geometry("640x720+{}+{}".format(24,24))
tkinter.Label(win,text="Halo!").grid()
win.after(100,GetWindowPos)
# win.deiconify()
win.mainloop()