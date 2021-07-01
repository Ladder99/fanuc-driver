xquery version "3.1" encoding "utf-8";

import module namespace functx="http://www.functx.com" at "http://www.xqueryfunctions.com/xq/functx-1.0.1-doc.xq";

(: https://tableconvert.com/ :)

element table {
    element tr {
        element td { "section" },
        element td { "name" },
        element td { "comment" },
        element td { "reference" },
        element td { "30i-B(M)" }, element td { "30i-B(T)" }, element td { "30i-B(LC)" }, element td { "30i-B(P)" }, element td { "30i-B(L)" }, element td { "30i-B(W)" },
        element td { "30i-A(M)" }, element td { "30i-A(T)" }, element td { "30i-A(LC)" }, element td { "30i-A(P)" }, element td { "30i-A(L)" }, element td { "30i-A(W)" },
        element td { "21i-B(M)" }, element td { "21i-B(T)" }, element td { "21i-B(LC)" },
        element td { "21i-A(M)" }, element td { "21i-A(T)" }, element td { "21i-A(LC)" },
        element td { "21(M)" }, element td { "21(T)" }, element td { "21(LC)" },
        element td { "18i-B(M)" }, element td { "18i-B(T)" }, element td { "18i-B(LC)" },
        element td { "18i-A(M)" }, element td { "18i-A(T)" }, element td { "18i-A(LC)" },
        element td { "18(M)" }, element td { "18(T)" }, element td { "18(LC)" },
        element td { "18i(P)" }, element td { "18i(L)" }, element td { "18i(W)" },
        element td { "16i-B(M)" }, element td { "16i-B(T)" }, element td { "16i-B(LC)" },
        element td { "16i-A(M)" }, element td { "16i-A(T)" }, element td { "16i-A(LC)" },
        element td { "16i(P)" }, element td { "16i(L)" }, element td { "16i(W)" },
        element td { "16(M)" }, element td { "16(T)" }, element td { "16(LC)" },
        element td { "15i(M)" }, element td { "15i(T)" }, element td { "15i(LC)" },
        element td { "15(M)" }, element td { "15(T)" }, element td { "15(LC)" },
        element td { "0i-F(M)" }, element td { "0i-F(T)" }, element td { "0i-F(LC)" }, element td { "0i-F(P)" }, element td { "0i-F(L)" }, element td { "0i-F(W)" },
        element td { "0i-D(M)" }, element td { "0i-D(T)" }, element td { "0i-D(LC)" }, element td { "0i-D(P)" }, element td { "0i-D(L)" }, element td { "0i-D(W)" },
        element td { "0i-B/C(M)" }, element td { "0i-B/C(T)" }, element td { "0i-B/C(LC)" },
        element td { "0i-A(M)" }, element td { "0i-A(T)" }, element td { "0i-A(LC)" },
        element td { "PMi-D" }, element td { "PMi-H" }, element td { "PMi-A" }
    },
    for $doc in collection('../SpecE/?select=*.xml;recurse=yes')
    let $section := lower-case(tokenize(document-uri($doc), "[/]")[last()-1])
    let $name := $doc/root/func/title/text() (:concat("[", $doc/root/func/title/text(), "](#", $doc/root/func/title/text(), ")"):)
    let $prototype := functx:trim(normalize-space(string-join($doc/root/func/declare/vc/prottype/string(), "")))
    let $references := string-join
        (
            for $reference in $doc/root/func/reference/item/name/text()
            return normalize-space($reference), ", "
            (:return concat("[", normalize-space($reference), "](#", normalize-space($reference), ")"), ", ":)
        )
    let $comment := 
        if (string-length(functx:trim($doc/root/func/doc/cmn[1]/text()[1])) > 0) then
            normalize-space($doc/root/func/doc/cmn[1]/text()[1])
        else
            normalize-space($doc/root/func/doc/cmn[1]/p[1]/text()[1])
    (:let $comment_full := $doc/root/func/doc:)
    let $support := $doc/root/func/support
    where $doc/root/func
    order by $section, $name
    return
        element tr {
            element td { $section },
            element td { $name },
            element td { $comment },
            element td { $references },
            element td { $support/m/fs30ib/string() }, element td { $support/t/fs30ib/string() }, element td { $support/lc/fs30ib/string() }, element td { $support/p/fs30ib/string() }, element td { $support/l/fs30ib/string() }, element td { $support/w/fs30ib/string() },
            element td { $support/m/fs30ia/string() }, element td { $support/t/fs30ia/string() }, element td { $support/lc/fs30ia/string() }, element td { $support/p/fs30ia/string() }, element td { $support/l/fs30ia/string() }, element td { $support/w/fs30ia/string() },
            element td { $support/m/fs21ib/string() }, element td { $support/t/fs21ib/string() }, element td { $support/lc/fs21ib/string() },
            element td { $support/m/fs21ia/string() }, element td { $support/t/fs21ia/string() }, element td { $support/lc/fs21ia/string() },
            element td { $support/m/fs21/string() }, element td { $support/t/fs21/string() }, element td { $support/lc/fs21/string() },
            element td { $support/m/fs18ib/string() }, element td { $support/t/fs18ib/string() }, element td { $support/lc/fs18ib/string() },
            element td { $support/m/fs18ia/string() }, element td { $support/t/fs18ia/string() }, element td { $support/lc/fs18ia/string() },
            element td { $support/m/fs18/string() }, element td { $support/t/fs18/string() }, element td { $support/lc/fs18/string() },
            element td { $support/p/fs18i/string() }, element td { $support/l/fs18i/string() }, element td { $support/w/fs18i/string() },
            element td { $support/m/fs16ib/string() }, element td { $support/t/fs16ib/string() }, element td { $support/lc/fs16ib/string() },
            element td { $support/m/fs16ia/string() }, element td { $support/t/fs16ia/string() }, element td { $support/lc/fs16ia/string() },
            element td { $support/p/fs16i/string() }, element td { $support/l/fs16i/string() }, element td { $support/w/fs16i/string() },
            element td { $support/m/fs16/string() }, element td { $support/t/fs16/string() }, element td { $support/lc/fs16/string() },
            element td { $support/m/fs15i/string() }, element td { $support/t/fs15i/string() }, element td { $support/lc/fs15i/string() },
            element td { $support/m/fs15/string() }, element td { $support/t/fs15/string() }, element td { $support/lc/fs15/string() },
            element td { $support/m/fs0if/string() }, element td { $support/t/fs0if/string() }, element td { $support/lc/fs0if/string() }, element td { $support/p/fs0if/string() }, element td { $support/l/fs0if/string() }, element td { $support/w/fs0if/string() },
            element td { $support/m/fs0id/string() }, element td { $support/t/fs0id/string() }, element td { $support/lc/fs0id/string() }, element td { $support/p/fs0id/string() }, element td { $support/l/fs0id/string() }, element td { $support/w/fs0id/string() },
            element td { $support/m/fs0ib/string() }, element td { $support/t/fs0ib/string() }, element td { $support/lc/fs0ib/string() },
            element td { $support/m/fs0ia/string() }, element td { $support/t/fs0ia/string() }, element td { $support/lc/fs0ia/string() },
            element td { $support/pw/d/string() }, element td { $support/pw/h/string() }, element td { $support/pw/a/string() }
        }
}


