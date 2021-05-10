def sign(x):
    if x == 0:
        return 0
    elif x > 0:
        return 1
    else:
        return -1


def f2p(expf, es):
    pes = 2**es
    reg = 0
    if expf > 0:
        reg = expf // pes
    elif expf < 0:
        reg = - (-expf+1) // pes
        print("- {0} // {1} = {2}".format(-expf+1, pes, reg))
    expp = expf - reg*pes
    return reg, expp


def px(reg, expp, es):
    return expp + reg*(2**es)

#for i in range(65):
#    r,e = f2p(i, 3)
#    print("{0} : r={1} e={2}".format(i, r, e))

#for i in range(-32, 33):
#    print("{0}//{1} = {2}".format(i, 8, i//8))

def clz(n, size=32):
    m = 1 << (size-1)
    c = 0
    while m & n == 0:
        c += 1
        m >>= 1
    return c

